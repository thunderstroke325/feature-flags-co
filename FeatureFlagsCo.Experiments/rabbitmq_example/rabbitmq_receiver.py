#!/usr/bin/env python
import logging
import os
import sys

from rabbitmq.rabbitmq import RabbitMQConsumer


class SimpleConsumer(RabbitMQConsumer):

    def handle_body(self, body):
        print(" [x] %r" % body)


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
