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
        print(" [redis] %r" % json.loads(self.redis.get(body['id']).decode()))


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    while True:
        try:
            SimpleConsumer().consumer(
                '', ('topic1', ['py.#']), ('topic2', ['py.#']))
            break
        except KeyboardInterrupt:
            logging.info('#######Interrupted#########')
            try:
                sys.exit(0)
            except SystemExit:
                os._exit(0)
        except Exception as e:
            logging.exception("#######unexpected#########")
