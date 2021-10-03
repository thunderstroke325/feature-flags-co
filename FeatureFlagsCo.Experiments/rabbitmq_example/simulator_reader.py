from config.config_handling import get_config_value
import logging
import os
import sys
from rabbitmq.rabbitmq import RabbitMQConsumer

logger = logging.getLogger("simulator_reader")
logger.setLevel(logging.INFO)


class Q3SimpleConsumer(RabbitMQConsumer):

    def handle_body(self, body, **properties):
        routing_key = properties.get(
            'routing_key', None) if properties else None
        if routing_key:
            logger.info("[Q3: Listen From -> ] %r" % routing_key)
        logger.info(" [mq] %r" % body)


if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    mq_host = get_config_value('rabbitmq', 'mq_host')
    mq_port = get_config_value('rabbitmq', 'mq_port')
    mq_username = get_config_value('rabbitmq', 'mq_username')
    mq_passwd = get_config_value('rabbitmq', 'mq_passwd')
    redis_host = get_config_value('redis', 'redis_host')
    redis_port = get_config_value('redis', 'redis_port')
    redis_passwd = get_config_value('redis', 'redis_passwd')
    consumer = Q3SimpleConsumer(mq_host,
                                mq_port,
                                mq_username,
                                mq_passwd,
                                redis_host,
                                redis_port,
                                redis_passwd).run('simulator.consumer', ('Q3', ['py.experiments.experiment.results.#']))
