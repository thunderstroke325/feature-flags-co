

import logging

from azure_service_bus.insight_utils import get_insight_logger
from experiment.health_check import HealthCheck


trace_health_check_logger = get_insight_logger('trace_azure_bus_health_check')
trace_health_check_logger.setLevel(logging.INFO)

debug_health_check_logger = logging.getLogger('debug_azure_bus_health_check')
debug_health_check_logger.setLevel(logging.INFO)


class AzureHealthCheck(HealthCheck):
    def __init__(self,
                 redis_host='localhost',
                 redis_port=6379,
                 redis_passwd='',
                 wait_timeout=180):
        try:
            ssl = True if int(redis_port) == 6380 else False
        except:
            ssl = False
        super().__init__(redis_host=redis_host,
                         redis_port=redis_port,
                         redis_passwd=redis_passwd,
                         ssl=ssl,
                         wait_timeout=wait_timeout,
                         trace_health_check_logger=trace_health_check_logger,
                         debug_health_check_logger=debug_health_check_logger)
