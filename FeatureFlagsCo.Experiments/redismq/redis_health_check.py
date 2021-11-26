
import logging

from azure_service_bus.insight_utils import get_insight_logger
from config.config_handling import get_config_value
from experiment.health_check import HealthCheck

engine = get_config_value('general', 'engine')
if engine == 'azure' or engine == 'redis':
    trace_health_check_logger = get_insight_logger('trace_redis_health_check')
else:
    trace_health_check_logger = logging.getLogger('trace_redis_health_check')
trace_health_check_logger.setLevel(logging.INFO)

debug_health_check_logger = logging.getLogger('debug_redis_health_check')
debug_health_check_logger.setLevel(logging.INFO)


class RedisHealthCheck(HealthCheck):

    def __init__(self,
                 redis_host='localhost',
                 redis_port=6379,
                 redis_passwd='',
                 redis_ssl=False,
                 wait_timeout=180):
        super().__init__(redis_host=redis_host,
                         redis_port=redis_port,
                         redis_passwd=redis_passwd,
                         ssl=redis_ssl,
                         wait_timeout=wait_timeout,
                         trace_health_check_logger=trace_health_check_logger,
                         debug_health_check_logger=debug_health_check_logger,
                         engine='redis')
