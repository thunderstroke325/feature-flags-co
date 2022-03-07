from abc import ABC, abstractmethod
from ctypes import Union

import redis
from redis.cluster import RedisCluster as rc


class RedisStub:

    def __init__(self, redis_host='localhost', redis_port=6379, redis_passwd='', ssl=False, mode='standalone'):
        self._redis_host = redis_host
        self._redis_port = redis_port
        self._redis_passwd = redis_passwd
        self._ssl = ssl
        self._mode = mode
        self._init__redis_connection(redis_host, redis_port, redis_passwd, ssl, mode)

    def _init__redis_connection(self, host, port, password, ssl, mode):
        if 'standalone' == mode:
            self._redis = redis.Redis(host=host,
                                      port=port,
                                      password=password,
                                      ssl=ssl,
                                      charset='utf-8',
                                      decode_responses=True)
        elif 'cluster' == mode:
            startup_nodes = [{"host": host, "port": port}]
            self._redis = rc(startup_nodes=startup_nodes,
                             password=password,
                             ssl=ssl,
                             charset='utf-8',
                             decode_responses=True,
                             skip_full_coverage_check=True)
        else:
            raise NotImplementedError("this mode is not supported")

    @property
    def redis(self) -> Union[redis.Redis, rc]:
        return self._redis


class Sender(ABC):

    @abstractmethod
    def send(self, *messages, **kwargs):
        pass


class Receiver(ABC):

    @abstractmethod
    def consume(self, **kwargs):
        pass


class MessageHandler(ABC):

    @abstractmethod
    def handle_body(self, body, **kwargs):
        pass
