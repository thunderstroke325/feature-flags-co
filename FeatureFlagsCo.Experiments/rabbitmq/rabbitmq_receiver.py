#!/usr/bin/env python
import json
import logging
import os
import sys

import redis

from rabbitmq.rabbitmq import RabbitMQConsumer


class SimpleConsumer(RabbitMQConsumer):

    def handle_body(self, body, **properties):
        routing_key = properties.get(
            'routing_key', None) if properties else None
        if routing_key:
            print(" [from] %r" % routing_key)
        print(" [mq] %r" % body)
        print(" [redis] %r" % self.redis_get(body['id']))


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    SimpleConsumer().run(
        'consumer', ('topic1', ['py.#']), ('topic5', ['py.#']))
