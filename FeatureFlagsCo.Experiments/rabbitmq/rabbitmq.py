import json
import logging
import os
import sys
import time
from abc import ABC, abstractmethod

import pika
import redis
from config.config_handling import get_config_value
from pika.adapters.blocking_connection import BlockingConnection

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
        self._conn = pika.BlockingConnection(
            pika.ConnectionParameters(host=host, port=port,
                                      credentials=credentials))

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
    def conn(self) -> BlockingConnection:
        return self._conn

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


class RabbitMQSender(RabbitMQ):

    def mq_close(self):
        try:
            if self.conn.is_open:
                self.conn.close()
        except:
            logger.exception('#######unexpected#########')

    def send(self, topic, routing_key, msg):
        for i in range(3):
            try:
                channel = self.conn.channel()
                channel.exchange_declare(
                    exchange=topic,
                    exchange_type='topic',
                    durable=True)
                logger.info("#######send topic: %r, routing_key: %r#######" % (
                    topic, routing_key))
                body = str.encode(json.dumps(msg))
                channel.basic_publish(
                    exchange=topic,
                    routing_key=routing_key,
                    body=body,
                    # make message persistent
                    properties=pika.BasicProperties(delivery_mode=2))
                channel.close()
                break
            except (pika.exceptions.ConnectionClosedByBroker, pika.exceptions.AMQPConnectionError):
                # Uncomment this to make the example not attempt recovery
                # from server-initiated connection closure, including
                # when the node is stopped cleanly
                #
                # Recover on all other connection errors
                logger.exception(
                    "Connection was closed by Broker or other raison, retrying...")
                time.sleep(1)
                self._init_mq_connection(
                    self._mq_host, self._mq_port, self._mq_username, self._mq_passwd)
                continue
            except:
                logger.exception('#######unexpected#########')
                if not isinstance(self, RabbitMQConsumer):
                    self.mq_close()
                break


class RabbitMQConsumer(ABC, RabbitMQSender):

    @abstractmethod
    def handle_body(self, body, **properties):
        pass

    def consumer(self, queue='', *bindings):
        channel = self.conn.channel()
        if bindings and len(bindings) > 0:
            if queue:
                exclusive = False
                result = channel.queue_declare(
                    queue, durable=True, exclusive=exclusive)
            else:
                exclusive = True
                result = channel.queue_declare('', exclusive=exclusive)
            queue_name = result.method.queue
            for topic, binding_keys in bindings:
                channel.exchange_declare(
                    exchange=topic,
                    exchange_type='topic',
                    durable=True)
                for binding_key in set(binding_keys):
                    channel.queue_bind(
                        exchange=topic, queue=queue_name, routing_key=binding_key)
                    logger.info(
                        "#######get topic: %r, queue: %r, routing_key: %r#######" % (topic, queue_name, binding_key))
                if not binding_keys:
                    channel.queue_bind(exchange=topic, queue=queue_name)
                    logger.info("#######get topic: %r, queue: %r#######" %
                                (topic, queue_name))

            channel.basic_qos(prefetch_count=1)
            for method, properties, body in channel.consume(queue_name,
                                                            auto_ack=False,
                                                            exclusive=exclusive):

                self.handle_body(json.loads(body.decode()),
                                 routing_key=method.routing_key,
                                 delivery=method.delivery_tag)
                channel.basic_ack(method.delivery_tag)
            channel.cancel()
            channel.close()
            # def callback(ch, method, properties, body):
            #     self.handle_body(json.loads(body.decode()),
            #                      routing_key=method.routing_key)
            # channel.basic_consume(
            #     queue=queue_name, on_message_callback=callback, auto_ack=True)
            # channel.start_consuming()

    def run(self, queue, *bindings):
        while True:
            try:
                if not self.conn or self.conn.is_closed:
                    self._init_mq_connection(
                        self._mq_host, self._mq_port, self._mq_username, self._mq_passwd)
                if not self.redis or not self.redis.ping():
                    self._init__redis_connection(
                        self._redis_host, self._redis_port, self._redis_passwd)
                self.consumer(queue, *bindings)
            except KeyboardInterrupt:
                logger.info('#######Interrupted#########')
                try:
                    sys.exit(0)
                except SystemExit:
                    os._exit(0)
            except (pika.exceptions.ConnectionClosedByBroker, pika.exceptions.AMQPConnectionError):
                # Uncomment this to make the example not attempt recovery
                # from server-initiated connection closure, including
                # when the node is stopped cleanly
                #
                # Recover on all other connection errors
                logger.exception(
                    "Connection was closed by Broker or other raison, retrying...")
                time.sleep(10)
            # Do not recover on channel errors
            except pika.exceptions.AMQPChannelError as err:
                logger.fatal(
                    "Caught a channel error: {}, stopping...".format(err))
                try:
                    sys.exit(1)
                except SystemExit:
                    os._exit(1)
            except:
                logger.exception('#######unexpected#########')
            finally:
                self.mq_close()
                self._conn = None
                self._redis = None
