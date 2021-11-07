import logging
import sys
from datetime import datetime
from time import sleep

import redis
from azure.servicebus.exceptions import OperationTimeoutError

from azure_service_bus.constants import FMT
from azure_service_bus.insight_utils import (get_custom_properties,
                                             get_insight_logger)
from azure_service_bus.utils import quite_app

trace_health_check_logger = get_insight_logger('trace_azure_bus_health_check')
trace_health_check_logger.setLevel(logging.INFO)

debug_health_check__logger = logging.getLogger('debug_azure_bus_health_check')
debug_health_check__logger.setLevel(logging.INFO)


class AzureHealthCheck:
    def __init__(self,
                 redis_host='localhost',
                 redis_port=6379,
                 redis_passwd='',
                 wait_timeout=180):
        self._redis_host = redis_host
        self._redis_port = redis_port
        self._redis_passwd = redis_passwd
        self._init__redis_connection(redis_host, redis_port, redis_passwd)
        self._wait_timeout = wait_timeout

    def _init__redis_connection(self, host, port, password):
        try:
            ssl = True if int(port) == 6380 else False
        except:
            ssl = False
        self._redis = redis.Redis(
            host=host,
            port=port,
            password=password,
            ssl=ssl,
            charset='utf-8',
            decode_responses=True)

    def check_health(self):
        debug_health_check__logger.info('################health check start################')
        for i in range(sys.maxsize):
            try:
                if i == 0:
                    self._redis.delete('topic_pulling_last_exec_time')
                sleep(self._wait_timeout)
                debug_health_check__logger.info('################Checking health...################')
                props = None
                for key, value in self._redis.hgetall('topic_pulling_last_exec_time').items():
                    props = get_custom_properties(topic=key)
                    interval = abs((datetime.utcnow() - datetime.strptime(value, FMT)).total_seconds())
                    if interval >= 960:
                        raise OperationTimeoutError(message=f'No reponse in {key} more than 16 mins, {key} is halting now, DO RESTART EXPT!!!!!')
                    elif interval >= 600:
                        raise OperationTimeoutError(message=f'No reponse in {key} more than 10 mins, {key} may be halting now, CHECK APP INSIGHTS!!!')
                    elif interval >= 300:
                        trace_health_check_logger.warning(f'No reponse in {key} more than 5 mins, CHECK APP INSIGHTS!', extra=props)
                    else:
                        trace_health_check_logger.info(f'HEALTH CHECK: {key} healthy', extra=props)
            except OperationTimeoutError as e:
                trace_health_check_logger.exception(e.message, extra=props)
            except redis.RedisError:
                debug_health_check__logger.exception('################unexpected################')
                if not self._redis.ping():
                    trace_health_check_logger.exception('CANNOT PING redis, trying to reconnect...')
                    self._init__redis_connection(self._redis_host,
                                                 self._redis_port,
                                                 self._redis_passwd)
            except KeyboardInterrupt:
                debug_health_check__logger.info('################Interrupted################')
                quite_app(0)
            except:
                trace_health_check_logger.exception('UNEXPECTED ERROR in HEALTHY CHECK')
                quite_app(1)

        quite_app(0)
