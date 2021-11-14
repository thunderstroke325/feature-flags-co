import logging

from azure_service_bus.insight_utils import get_insight_logger

from azure_service_bus.send_consume import AzureReceiver
from experiment.utils import decode, encode, get_custom_properties

foo_logger = get_insight_logger()
foo_logger.setLevel(logging.INFO)


class FooReceiver(AzureReceiver):
    def handle_body(self, body, **kwargs):
        instance_name = kwargs.pop('instance_name', '')
        instance_id = kwargs.pop('instance_id', '')
        print('topic: %s' % instance_name)
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

        foo_logger.info('FOO', extra=get_custom_properties(topic=instance_name, instance=f'{instance_name}-{instance_id}'))
