import logging
import os
from experiment.utils import get_custom_properties
from redismq.send_consume import RedisReceiver


class FooReceiver(RedisReceiver):
    def handle_body(self, body, **kwargs):
        instance_name = kwargs.pop('instance_name', '')
        instance_id = kwargs.pop('instance_id', '')
        trace_logger = kwargs.pop('trace_logger', logging.getLogger(__name__))
        print('topic: %s' % instance_name)
        print('mq received: %s' % body)
        trace_logger.info('REDIS-FOO', extra=get_custom_properties(topic=instance_name, instance=f'{instance_name}-{instance_id}'))


TOPIC_NAME = 'ds'

if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    process_name = os.path.basename(__file__)
    receriver = FooReceiver()
    receriver.consume(topic=TOPIC_NAME, process_name=process_name)
