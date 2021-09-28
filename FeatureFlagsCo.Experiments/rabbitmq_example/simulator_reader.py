import logging
import os
import sys
from rabbitmq.rabbitmq import RabbitMQConsumer


class Q3SimpleConsumer(RabbitMQConsumer):

    def handle_body(self, body, **properties):
        routing_key = properties.get(
            'routing_key', None) if properties else None
        if routing_key:
            print("[Q3: Listen From -> ] %r" % routing_key)
        print(" [mq] %r" % body)


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    consumer = Q3SimpleConsumer().run(
        'simulator.consumer', ('Q3', ['py.experiments.experiment.results.#']))
