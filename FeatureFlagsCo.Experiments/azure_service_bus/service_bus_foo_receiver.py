import logging

from azure_service_bus.insight_utils import (get_custom_properties,
                                             get_insight_logger)

from azure_service_bus.send_consume import AzureReceiver
from azure_service_bus.utils import decode, encode

foo_logger = get_insight_logger()
foo_logger.setLevel(logging.INFO)


class FooReceiver(AzureReceiver):
    def handle_body(self, instance_id, topic, body):
        print('topic: %s' % topic)
        print('mq received: %s' % body)
        for value in self.redis.lrange('FOO_LIST_KEY', 0, -1):
            print('redis list received: %s' % decode(value))

        for value in self.redis.smembers('FOO_SET_KEY'):
            print('redis set received: %s' % decode(value))
        self.redis.srem('FOO_SET_KEY', encode(body))
        num = self.redis.scard('FOO_SET_KEY')
        print('num of element in set: %s' % num)

        with self.redis.pipeline() as pipeline:
            pipeline.delete('FOO_LIST_KEY')
            pipeline.delete('FOO_SET_KEY')
            pipeline.execute()

        foo_logger.info('FOO', extra=get_custom_properties(topic=topic, instance=f'{topic}-{instance_id}'))
