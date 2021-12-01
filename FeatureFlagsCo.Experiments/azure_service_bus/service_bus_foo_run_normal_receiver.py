
import logging
import os

from config.config_handling import get_config_value

from azure_service_bus.service_bus_foo_receiver import FooReceiver

TOPIC_NAME = 'ds'
SUBSCRIPTION_NAME = 'py'

if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    sb_host = get_config_value('azure', 'fully_qualified_namespace')
    sb_sas_policy = get_config_value('azure', 'sas_policy')
    sb_sas_key = get_config_value('azure', 'servicebus_sas_key')
    process_name = os.path.basename(__file__)
    FooReceiver(sb_host, sb_sas_policy, sb_sas_key, special_topic=TOPIC_NAME).consume(process_name=process_name,
                                                                                      topic=(TOPIC_NAME, SUBSCRIPTION_NAME),
                                                                                      prefetch_count=10,
                                                                                      is_dlq=False)
