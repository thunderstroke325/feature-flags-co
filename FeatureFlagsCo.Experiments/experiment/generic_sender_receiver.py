from abc import ABC, abstractmethod
import redis


class RedisStub:
    def __init__(self,
                 redis_host='localhost',
                 redis_port=6379,
                 redis_passwd='',
                 ssl=False):
        self._redis_host = redis_host
        self._redis_port = redis_port
        self._redis_passwd = redis_passwd
        self._ssl = ssl
        self._init__redis_connection(redis_host, redis_port, redis_passwd, ssl)

    def _init__redis_connection(self, host, port, password, ssl):
        self._redis = redis.Redis(
            host=host,
            port=port,
            password=password,
            ssl=ssl,
            charset='utf-8',
            decode_responses=True)

    @property
    def redis(self) -> redis.Redis:
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
