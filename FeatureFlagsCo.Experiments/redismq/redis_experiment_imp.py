
from experiment.generic_p1 import P1GetExptRecordingInfo
from experiment.generic_p2 import P2GetExptResult
from experiment.generic_p3 import P3GetExptFFEvents, P3GetExptUserEvents
from redismq.send_consume import RedisReceiver


class P1RedisGetExptRecordingInfoReceiver(RedisReceiver, P1GetExptRecordingInfo):
    pass


class P2RedisGetExptResultReceiver(RedisReceiver, P2GetExptResult):
    def __init__(self,
                 redis_host='localhost',
                 redis_port='6379',
                 redis_passwd=None,
                 redis_ssl=False,
                 wait_timeout=30.0):
        super().__init__(redis_host, redis_port, redis_passwd, ssl=redis_ssl)
        self._init_wait_timeout(wait_timeout=wait_timeout)


class P3RedisGetExptFFEventsReceiver(RedisReceiver, P3GetExptFFEvents):
    pass


class P3RedisGetExptUserEventsReceiver(RedisReceiver, P3GetExptUserEvents):
    pass
