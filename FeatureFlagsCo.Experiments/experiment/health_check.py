import logging
import sys
from datetime import datetime
from time import sleep
from xmlrpc.client import ServerProxy

import redis

from experiment.constants import (AZURE_HEALTH_CHECK_TIMEOUT,
                                  DEFAULT_HEALTH_CHECK_TIMEOUT, FMT,
                                  get_azure_instance_id)
from experiment.generic_sender_receiver import RedisStub
from experiment.utils import decode, encode, get_custom_properties, quite_app


class HealthCheck(RedisStub):
    def __init__(self,
                 redis_host='localhost',
                 redis_port=6379,
                 redis_passwd='',
                 ssl=False,
                 wait_timeout=180,
                 rpc_server_url='http://localhost:9001/RPC2',
                 trace_health_check_logger=None,
                 debug_health_check_logger=None,
                 engine='azure'):
        super().__init__(redis_host, redis_port, redis_passwd, ssl)
        self._wait_timeout = wait_timeout
        self._server = ServerProxy(rpc_server_url)
        self._trace_logger = trace_health_check_logger if trace_health_check_logger else logging.getLogger('trace_health_check')
        self._debug_logger = debug_health_check_logger if debug_health_check_logger else logging.getLogger('debug_health_check')
        self._timeout = AZURE_HEALTH_CHECK_TIMEOUT if engine == 'azure' else DEFAULT_HEALTH_CHECK_TIMEOUT

    def _stop_process(self, process_name, retries=3):
        for _ in range(retries):
            try:
                if self._server.supervisor.stopProcess(process_name):
                    return True
            except:
                pass
        return False

    def _restart_process(self, process_name, retries=3):
        for _ in range(retries):
            try:
                if self._server.supervisor.stopProcess(process_name):
                    if self._server.supervisor.startProcess(process_name):
                        return True
            except:
                pass
        return False

    def check_health(self):
        self._debug_logger.info('################health check start################')
        machine_id = get_azure_instance_id()
        check_health_id = f'topic_pulling_last_exec_time_in_{machine_id}'
        for i in range(sys.maxsize):
            try:
                if i == 0:
                    self._redis.delete(check_health_id)
                sleep(self._wait_timeout)
                self._debug_logger.info('################Checking health...################')
                props = None
                dict_receivers = self._redis.hgetall(check_health_id)
                for key, value in dict_receivers.items():
                    dict_value = decode(value)  # {'topic': topic, 'instance': instance, 'datetime': datetime.utcnow().strftime(FMT)}
                    topic, instance_id, pulling_time = dict_value.get('topic', ''), dict_value.get('instance', ''), dict_value.get('datetime', False)
                    props = get_custom_properties(topic=topic, instance=f'{topic}-{instance_id}')
                    if pulling_time:
                        interval = abs((datetime.utcnow() - datetime.strptime(pulling_time, FMT)).total_seconds())
                        if interval >= self._timeout:
                            if self._restart_process(key):
                                dict_value['datetime'] = datetime.utcnow().strftime(FMT)
                                self.redis.hset(check_health_id, key, encode(dict_value))
                                self._trace_logger.error(f'No reponse in {topic}-{instance_id} more than {self._timeout}s, {topic}-{instance_id} is restarted by sys!!!', extra=props)
                            else:
                                raise TimeoutError(f'No reponse in {topic}-{instance_id} more than {self._timeout}s, {topic}-{instance_id} is halting now, DO RESTART EXPT!!!!!')
                        else:
                            self._trace_logger.info(f'HEALTH CHECK: {topic}-{instance_id} healthy', extra=props)
                    else:
                        self._stop_process(key)
                        raise ValueError('last exec time NOT FOUND')
            except TimeoutError as e:
                self._trace_logger.exception(str(e), extra=props)
            except ValueError as e:
                self._trace_logger.exception(str(e), extra=props)
            except redis.RedisError as e:
                self._trace_logger.exception(str(e), extra=props)
                try:
                    self.redis.ping()
                except:
                    self._trace_logger.exception('CANNOT PING redis, trying to reconnect...')
                    self._init__redis_connection(self._redis_host,
                                                 self._redis_port,
                                                 self._redis_passwd,
                                                 self._ssl)
            except KeyboardInterrupt:
                self._debug_logger.info('################Interrupted################')
                quite_app(0)
            except:
                self._trace_logger.exception('UNEXPECTED ERROR in HEALTHY CHECK')
                quite_app(1)

        quite_app(0)
