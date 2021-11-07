import logging
from azure_service_bus.azure_bus_health_check import AzureHealthCheck
from config.config_handling import get_config_value

if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    redis_host = get_config_value('redis', 'redis_host')
    redis_port = get_config_value('redis', 'redis_port')
    redis_passwd = get_config_value('redis', 'redis_passwd')
    try:
        wait_timeout = float(get_config_value('p2', 'wait_timeout'))
    except:
        wait_timeout = 30.0
    AzureHealthCheck(redis_host, redis_port, redis_passwd, wait_timeout).check_health()
