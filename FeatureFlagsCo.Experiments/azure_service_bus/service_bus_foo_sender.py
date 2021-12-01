import logging
from azure_service_bus.send_consume import AzureSender
from config.config_handling import get_config_value


TOPIC_NAME = 'ds'
ORIGIN = 'py'
E1 = 'expt1'
E2 = 'expt2'

if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    sb_host = get_config_value('azure', 'fully_qualified_namespace')
    sb_sas_policy = get_config_value('azure', 'sas_policy')
    sb_sas_key = get_config_value('azure', 'servicebus_sas_key')
    sender = AzureSender(sb_host, sb_sas_policy, sb_sas_key)
    sender.send(E1, E1, E1, E2, E1, topic=TOPIC_NAME, subscription=ORIGIN)
