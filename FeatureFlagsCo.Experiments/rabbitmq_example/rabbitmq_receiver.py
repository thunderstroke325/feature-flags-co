#!/usr/bin/env python
import json
import logging
import os
import sys

import redis

from rabbitmq.rabbitmq import RabbitMQConsumer


class SimpleConsumer(RabbitMQConsumer):

    def handle_body(self, body):
        print(" [mq] %r" % body)
        print(" [redis] %r" % json.loads(self.redis.get(body['id']).decode()))

if __name__ == '__main__':
    logging.basicConfig(encoding='utf-8', level=logging.INFO)
    try:
        SimpleConsumer().consumer('topic', 'py.1')
    except KeyboardInterrupt:
        logging.info('Interrupted')
        try:
            sys.exit(0)
        except SystemExit:
            os._exit(0)
