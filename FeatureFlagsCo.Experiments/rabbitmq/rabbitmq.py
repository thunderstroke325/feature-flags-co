import json
import logging
from abc import ABC, abstractmethod

import pika
import redis
from pika.adapters.blocking_connection import BlockingChannel

from config.config_handling import get_config_value


class RabbitMQ:
    def __int__(self):
        _mq_host = get_config_value('rabbitmq', 'mq_host')
        _mq_port = get_config_value('rabbitmq', 'mq_port')
        _mq_username = get_config_value('rabbitmq', 'mq_username')
        _mq_passwd = get_config_value('rabbitmq', 'mq_passwd')
        _redis_host = get_config_value('redis', 'redis_host')
        _redis_port = get_config_value('redis', 'redis_port')
        _redis_passwd = get_config_value('redis', 'redis_passwd')
        self.__init__(_mq_host, _mq_port, _mq_username, _mq_passwd, _redis_host,
                      _redis_port, _redis_passwd)

    def __init__(self,
                 mq_host='localhost',
                 mq_port=5672,
                 mq_username='guest',
                 mq_passwd='guest',
                 redis_host='localhost',
                 redis_port='6379',
                 redis_passwd=None
                 ):
        credentials = pika.PlainCredentials(mq_username, mq_passwd)
        self._channel = pika.BlockingConnection(
            pika.ConnectionParameters(host=mq_host, port=mq_port,
                                      credentials=credentials)).channel()
        self._redis = redis.Redis(
            host=redis_host, port=redis_port, password=redis_passwd)

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
        self.redis.delete(id)


class RabbitMQConsumer(ABC, RabbitMQ):

    @abstractmethod
    def handle_body(self, body, **properties):
        pass

    def consumer(self, queue='', *bindings):
        if bindings and len(bindings) > 0:
            if queue:
                result = self.channel.queue_declare(queue, durable=True)
            else:
                result = self.channel.queue_declare('', exclusive=True)
            queue_name = result.method.queue
            for topic, binding_keys in bindings:
                self.channel.exchange_declare(
                    exchange=topic, exchange_type='topic')
                if queue:
                    # direct queue mode
                    queue_name = result.method.queue
                    super().channel.queue_bind(
                        exchange=topic, queue=queue_name)
                    logging.info(
                        "topic: %r, queue: %r, direct queue mode" % (topic, queue_name))
                else:
                    # topic mode
                    for binding_key in set(binding_keys):
                        super().channel.queue_bind(
                            exchange=topic, queue=queue_name, routing_key=binding_key)
                        logging.info("topic: %r, queue: %r, binding_key: %r, topic mode" % (
                            topic, queue_name, binding_key))

        def callback(ch, method, properties, body):
            self.handle_body(json.loads(body.decode()),
                             routing_key=method.routing_key)

        self.channel.basic_consume(
            queue=queue_name, on_message_callback=callback, auto_ack=True)
        self.channel.start_consuming()


class RabbitMQSender(RabbitMQ):

    def send(self, topic, routing_key, *jsons):
        self.channel.exchange_declare(
            exchange=topic, exchange_type='topic')
        logging.info("topic: %r, routing_key: %r" % (topic, routing_key))
        for json_body in jsons:
            body = str.encode(json.dumps(json_body))
            self.channel.basic_publish(
                exchange=topic, routing_key=routing_key, body=body)

        self.channel.connection.close()
