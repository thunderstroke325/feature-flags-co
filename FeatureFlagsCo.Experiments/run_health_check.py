import logging
from distutils.util import strtobool
import os

from azure_service_bus.azure_bus_health_check import AzureHealthCheck
from config.config_handling import get_config_value
from redismq.redis_health_check import RedisHealthCheck

if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    engine = get_config_value('general', 'engine')
    redis_host = get_config_value('redis', 'redis_host')
    redis_port = get_config_value('redis', 'redis_port')
    redis_passwd = get_config_value('redis', 'redis_passwd')
    try:
        redis_ssl = strtobool(get_config_value('redis', 'redis_ssl'))
        wait_timeout = float(get_config_value('p2', 'wait_timeout'))
    except:
        redis_ssl = False
        wait_timeout = 30.0
    if engine == 'azure':
        AzureHealthCheck(redis_host, redis_port, redis_passwd, wait_timeout).check_health()
    elif engine == 'redis':
        RedisHealthCheck(redis_host, redis_port, redis_passwd, redis_ssl, wait_timeout).check_health()
    else:
        redis_host = cus_redis_host if (cus_redis_host := os.getenv('CUSTOMERS_HOST', False)) else redis_host
        redis_port = cus_redis_port if (cus_redis_port := os.getenv('CUSTOMERS_PORT', False)) else redis_port
        redis_passwd = cus_redis_passwd if (cus_redis_passwd := os.getenv('CUSTOMERS_PASSWD', False)) else redis_passwd
        try:
            if (cus_redis_ssl := os.getenv('CUSTOMERS_SSL', False)):
                redis_ssl = strtobool(cus_redis_ssl)
        except:
            pass
        RedisHealthCheck(redis_host, redis_port, redis_passwd, redis_ssl, wait_timeout).check_health()
