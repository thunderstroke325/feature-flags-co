import json
import logging
from abc import ABC, abstractmethod
import os
import sys


import pika
import redis
from pika.adapters.blocking_connection import BlockingChannel

from config.config_handling import get_config_value

logger = logging.getLogger("rabbitmq")
logger.setLevel(logging.INFO)


class RabbitMQ:

    def __init__(self,
                 mq_host='localhost',
                 mq_port=5672,
                 mq_username='guest',
                 mq_passwd='guest',
                 redis_host='localhost',
                 redis_port=6379,
                 redis_passwd=''
                 ):
        self._mq_host = mq_host
        self._mq_port = mq_port
        self._mq_username = mq_username
        self._mq_passwd = mq_passwd
        self._redis_host = redis_host
        self._redis_port = redis_port
        self._redis_passwd = redis_passwd
        self._init_mq_connection(mq_host, mq_port, mq_username, mq_passwd)
        self._init__redis_connection(
            redis_host, redis_port, redis_passwd)

    def _init_mq_connection(self,
                            host,
                            port,
                            username,
                            passwd):
        credentials = pika.PlainCredentials(username, passwd)
        self._channel = pika.BlockingConnection(
            pika.ConnectionParameters(host=host, port=port,
                                      credentials=credentials)).channel()

    def _init__redis_connection(self, host, port, password):
        try:
            ssl = True if int(port) == 6380 else False
        except:
            ssl = False
        self._redis = redis.Redis(
            host=host,
            port=port,
            password=password,
            ssl=ssl)

    @property
    def channel(self) -> BlockingChannel:
        return self._channel

    @property
    def redis(self) -> redis.Redis:
        return self._redis

    def redis_get(self, id):
        value = self.redis.get(id)
        return json.loads(value.decode()) if value else None

    def redis_set(self, id, value):
        self.redis.set(id, str.encode(json.dumps(value)))

    def redis_del(self, *id):
        self.redis.delete(*id)


class RabbitMQConsumer(ABC, RabbitMQ):

    @abstractmethod
    def handle_body(self, body, **properties):
        pass

    def consumer(self, queue='', *bindings):
        if bindings and len(bindings) > 0:
            if queue:
                exclusive = False
                result = self.channel.queue_declare(
                    queue, durable=True, exclusive=exclusive)
            else:
                exclusive = True
                result = self.channel.queue_declare('', exclusive=exclusive)
            queue_name = result.method.queue
            for topic, binding_keys in bindings:
                self.channel.exchange_declare(
                    exchange=topic, exchange_type='topic')
                for binding_key in set(binding_keys):
                    super().channel.queue_bind(
                        exchange=topic, queue=queue_name, routing_key=binding_key)
                    logger.info(
                        "#######get topic: %r, queue: %r, routing_key: %r#######" % (topic, queue_name, binding_key))
                if not binding_keys:
                    super().channel.queue_bind(exchange=topic, queue=queue_name)
                    logger.info("#######get topic: %r, queue: %r#######" %
                                (topic, queue_name))

            self.channel.basic_qos(prefetch_count=1)
            for method, properties, body in self.channel.consume(queue_name,
                                                                 auto_ack=False,
                                                                 exclusive=exclusive):

                self.handle_body(json.loads(body.decode()),
                                 routing_key=method.routing_key,
                                 delivery=method.delivery_tag)
                self.channel.basic_ack(method.delivery_tag)
                # def callback(ch, method, properties, body):
                #     self.handle_body(json.loads(body.decode()),
                #                      routing_key=method.routing_key)
                # self.channel.basic_consume(
                #     queue=queue_name, on_message_callback=callback, auto_ack=True)
                # self.channel.start_consuming()

    def run(self, queue, *bindings):
        while True:
            try:
                if not self.channel or self.channel.is_closed:
                    self._init_mq_connection(
                        self._mq_host, self._mq_port, self._mq_username, self._mq_passwd)
                if not self.redis or not self.redis.ping():
                    self._init__redis_connection(
                        self._redis_host, self._redis_port, self._redis_passwd)
                self.consumer(queue, *bindings)
                break
            except KeyboardInterrupt:
                logger.info('#######Interrupted#########')
                try:
                    sys.exit(0)
                except SystemExit:
                    os._exit(0)
            except:
                logger.exception('#######unexpected#########')
            finally:
                try:
                    if self.channel.connection.is_open:
                        self.channel.connection.close()
                except:
                    logger.exception('#######unexpected#########')
                finally:
                    self._channel = None
                    self._redis = None


class RabbitMQSender(RabbitMQ):

    def send(self, topic, routing_key, *jsons):
        self.channel.exchange_declare(
            exchange=topic, exchange_type='topic')
        logger.info("#######send topic: %r, routing_key: %r#######" %
                    (topic, routing_key))
        try:
            for json_body in jsons:
                body = str.encode(json.dumps(json_body))
                self.channel.basic_publish(
                    exchange=topic,
                    routing_key=routing_key,
                    body=body,
                    # make message persistent
                    properties=pika.BasicProperties(delivery_mode=2)
                )
        except:
            logger.exception('#######unexpected#########')
        finally:
            try:
                if self.channel.connection.is_open:
                    self.channel.connection.close()
            except:
                logger.exception('#######unexpected#########')
            finally:
                self._channel = None
                self._redis = None
