
import logging

from azure_service_bus.service_bus_foo_receiver import FooReceiver
from config.config_handling import get_config_value

TOPIC_NAME = 'ds'
SUBSCRIPTION_NAME = 'py'

if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    sb_host = get_config_value('azure', 'fully_qualified_namespace')
    sb_sas_policy = get_config_value('azure', 'sas_policy')
    sb_sas_key = get_config_value('azure', 'servicebus_sas_key')
    FooReceiver(sb_host, sb_sas_policy, sb_sas_key).consume(
        (TOPIC_NAME, SUBSCRIPTION_NAME), is_dlq=False)
