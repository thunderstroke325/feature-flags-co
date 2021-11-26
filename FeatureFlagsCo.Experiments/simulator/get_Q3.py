import logging
from distutils.util import strtobool

from azure_service_bus.send_consume import AzureReceiver
from config.config_handling import get_config_value
from redismq.send_consume import RedisReceiver

logger = logging.getLogger('azure_simulator_get_Q3')
logger.setLevel(logging.INFO)


class AzureSimulatorQ3Receiver(AzureReceiver):
    def handle_body(self, body, **kwargs):
        logger.info(" [mq] %r" % body)


class RedisSimpleQ3Receiver(RedisReceiver):
    def handle_body(self, body, **kwargs):
        logger.info(" [mq] %r" % body)


if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    engine = get_config_value('general', 'engine')
    sb_host = get_config_value('azure', 'fully_qualified_namespace')
    sb_sas_policy = get_config_value('azure', 'sas_policy')
    sb_sas_key = get_config_value('azure', 'servicebus_sas_key')
    redis_host = get_config_value('redis', 'redis_host')
    redis_port = get_config_value('redis', 'redis_port')
    redis_passwd = get_config_value('redis', 'redis_passwd')
    topic = get_config_value('p2', 'topic_Q3')
    subscription = get_config_value('p2', 'subscription_Q3')
    redis_ssl = strtobool(get_config_value('redis', 'redis_ssl'))
    if engine == 'azure':
        AzureSimulatorQ3Receiver(sb_host, sb_sas_policy, sb_sas_key, redis_host, redis_port, redis_passwd) \
            .consume(process_name='',
                     topic=(topic, subscription),
                     prefetch_count=50,
                     is_dlq=False)
    else:
        RedisSimpleQ3Receiver(redis_host, redis_port, redis_passwd, redis_ssl).consume(process_name='', topic=topic)
