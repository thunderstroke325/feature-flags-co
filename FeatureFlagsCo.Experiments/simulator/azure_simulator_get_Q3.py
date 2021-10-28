import logging
from azure_service_bus.send_consume import AzureReceiver
from config.config_handling import get_config_value

logger = logging.getLogger('azure_simulator_get_Q3')
logger.setLevel(logging.INFO)


class AzureSimulatorQ3Receiver(AzureReceiver):
    def handle_body(self, topic, body):
        logger.info(" [mq] %r" % body)


if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    sb_host = get_config_value('azure', 'fully_qualified_namespace')
    sb_sas_policy = get_config_value('azure', 'sas_policy')
    sb_sas_key = get_config_value('azure', 'servicebus_sas_key')
    redis_host = get_config_value('redis', 'redis_host')
    redis_port = get_config_value('redis', 'redis_port')
    redis_passwd = get_config_value('redis', 'redis_passwd')
    topic = get_config_value('p2', 'topic_Q3')
    subscription = get_config_value('p2', 'subscription_Q3')
    AzureSimulatorQ3Receiver(sb_host, sb_sas_policy, sb_sas_key, redis_host, redis_port, redis_passwd).consume(
        (topic, subscription), is_dlq=False)
