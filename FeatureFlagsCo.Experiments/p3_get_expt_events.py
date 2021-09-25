
import logging
import os
import sys
from rabbitmq.rabbitmq import RabbitMQConsumer


class P3GetEventsConsumer(RabbitMQConsumer):

    def handle_body(self, body, **properties):
        pass


if __name__ == '__main__':
    FORMAT = '%(asctime)-15s %(clientip)s %(user)-8s %(message)s'
    logging.basicConfig(format=FORMAT, encoding='utf-8', level=logging.INFO)
    while True:
        try:
            P3GetEventsConsumer().consumer(
                '', ('Q4', ['py.experiments.events.ff.#']), ('Q5', ['py.experiments.events.user.#']))
            break
        except KeyboardInterrupt:
            logging.info('#######Interrupted#########')
            try:
                sys.exit(0)
            except SystemExit:
                os._exit(0)
        except Exception as e:
            logging.exception("#######unexpected#########")
