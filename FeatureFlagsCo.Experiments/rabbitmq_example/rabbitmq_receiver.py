#!/usr/bin/env python
import logging
import os
import sys

import redis

from rabbitmq.rabbitmq import RabbitMQConsumer


class SimpleConsumer(RabbitMQConsumer):

    def handle_body(self, body):
        r = redis.Redis(host='localhost', port=6379)
        print(" [mq] %r" % body)
        print(" [redis] %r" % r.get(body['id']).decode())


if __name__ == '__main__':
    logging.basicConfig(encoding='utf-8', level=logging.INFO)
    try:
        SimpleConsumer('localhost').consumer('topic_ff', '', 'es.#')
    except KeyboardInterrupt:
        logging.info('Interrupted')
        try:
            sys.exit(0)
        except SystemExit:
            os._exit(0)
