import logging
import os
import sys
from distutils.util import strtobool

from azure_service_bus.azure_service_bus_experiment_imp import \
    P3AzureGetExptUserEventsReceiver as azure_sb_p3
from config.config_handling import get_config_value
from redismq.redis_experiment_imp import \
    P3RedisGetExptUserEventsReceiver as redis_sb_p3

if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    engine = get_config_value('general', 'engine')
    redis_host = get_config_value('redis', 'redis_host')
    redis_port = get_config_value('redis', 'redis_port')
    redis_passwd = get_config_value('redis', 'redis_passwd')
    topic = get_config_value('p3', 'topic_Q5')
    subscription = get_config_value('p3', 'subscription_Q5')
    try:
        redis_ssl = strtobool(get_config_value('redis', 'redis_ssl'))
        prefetch_count = int(get_config_value('p3', 'prefetch_count'))
    except:
        redis_ssl = False
        prefetch_count = 10
    process_name = ''
    if len(sys.argv) > 1:
        process_name = sys.argv[1]
    else:
        process_name = os.path.basename(__file__)
    if engine == 'azure':
        sb_host = get_config_value('azure', 'fully_qualified_namespace')
        sb_sas_policy = get_config_value('azure', 'sas_policy')
        sb_sas_key = get_config_value('azure', 'servicebus_sas_key')
        azure_sb_p3(sb_host, sb_sas_policy, sb_sas_key, redis_host, redis_port, redis_passwd) \
            .consume(process_name=process_name,
                     topic=(topic, subscription),
                     prefetch_count=prefetch_count,
                     is_dlq=False)
    elif engine == 'redis':
        redis_sb_p3(redis_host, redis_port, redis_passwd, redis_ssl).consume(process_name=process_name, topic=topic)
    else:
        redis_host = cus_redis_host if (cus_redis_host := os.getenv('CUSTOMERS_HOST', False)) else redis_host
        redis_port = cus_redis_port if (cus_redis_port := os.getenv('CUSTOMERS_PORT', False)) else redis_port
        redis_passwd = cus_redis_passwd if (cus_redis_passwd := os.getenv('CUSTOMERS_PASSWD', False)) else redis_passwd
        try:
            if (cus_redis_ssl := os.getenv('CUSTOMERS_SSL', False)):
                redis_ssl = strtobool(cus_redis_ssl)
        except:
            pass
        redis_sb_p3(redis_host, redis_port, redis_passwd, redis_ssl).consume(process_name=process_name, topic=topic)
