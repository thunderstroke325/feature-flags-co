import logging
import os
import sys
from distutils.util import strtobool

from azure_service_bus.azure_service_bus_experiment_imp import \
    P2AzureGetExptResultReceiver as aure_sb_p2
from config.config_handling import get_config_value
from redismq.redis_experiment_imp import \
    P2RedisGetExptResultReceiver as redis_p2

if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    engine = get_config_value('general', 'engine')
    redis_host = get_config_value('redis', 'redis_host')
    redis_port = get_config_value('redis', 'redis_port')
    redis_passwd = get_config_value('redis', 'redis_passwd')
    redis_mode = get_config_value('redis', 'redis_mode')
    topic = get_config_value('p2', 'topic_Q2')
    subscription = get_config_value('p2', 'subscription_Q2')
    try:
        redis_ssl = strtobool(get_config_value('redis', 'redis_ssl'))
        wait_timeout = float(get_config_value('p2', 'wait_timeout'))
        prefetch_count = int(get_config_value('p2', 'prefetch_count'))
    except:
        redis_ssl = False
        wait_timeout = 30.0
        prefetch_count = 1
    process_name = ''
    if len(sys.argv) > 1:
        process_name = sys.argv[1]
    else:
        process_name = os.path.basename(__file__)
    if engine == 'azure':
        sb_host = get_config_value('azure', 'fully_qualified_namespace')
        sb_sas_policy = get_config_value('azure', 'sas_policy')
        sb_sas_key = get_config_value('azure', 'servicebus_sas_key')
        aure_sb_p2(sb_host, sb_sas_policy, sb_sas_key, redis_host, redis_port, redis_passwd, redis_mode, wait_timeout) \
            .consume(process_name=process_name,
                     topic=(topic, subscription),
                     prefetch_count=prefetch_count,
                     is_dlq=False)
    elif engine == 'redis':
        redis_p2(redis_host, redis_port, redis_passwd, redis_ssl, redis_mode, wait_timeout) \
            .consume(process_name=process_name,
                     topic=topic,
                     fetch_mode='single')
    else:
        redis_host = cus_redis_host if (cus_redis_host := os.getenv('CUSTOMERS_HOST', False)) else redis_host
        redis_port = cus_redis_port if (cus_redis_port := os.getenv('CUSTOMERS_PORT', False)) else redis_port
        redis_passwd = cus_redis_passwd if (cus_redis_passwd := os.getenv('CUSTOMERS_PASSWD', False)) else redis_passwd
        redis_mode = cus_redis_mode if (cus_redis_mode := os.getenv('CUSTOMERS_MODE', False)) else redis_mode
        try:
            if (cus_redis_ssl := os.getenv('CUSTOMERS_SSL', False)):
                redis_ssl = strtobool(cus_redis_ssl)
        except:
            pass
        redis_p2(redis_host, redis_port, redis_passwd, redis_ssl, redis_mode, wait_timeout) \
            .consume(process_name=process_name,
                     topic=topic,
                     fetch_mode='single')
