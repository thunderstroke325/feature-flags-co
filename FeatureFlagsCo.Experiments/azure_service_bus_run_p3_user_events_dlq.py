import logging
from azure_service_bus.p3_azure_service_bus_get_expt_events import P3AzureGetExptUserEventsReceiver
from config.config_handling import get_config_value

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
    topic = get_config_value('p3', 'topic_Q5')
    subscription = get_config_value('p3', 'subscription_Q5')

    P3AzureGetExptUserEventsReceiver(sb_host, sb_sas_policy, sb_sas_key, redis_host, redis_port, redis_passwd).consume(
        (topic, subscription), is_dlq=True)
