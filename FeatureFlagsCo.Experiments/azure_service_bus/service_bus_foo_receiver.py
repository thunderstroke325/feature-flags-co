import logging

from azure_service_bus.insight_utils import get_insight_logger

from azure_service_bus.send_consume import AzureReceiver
from experiment.utils import get_custom_properties

foo_logger = get_insight_logger()
foo_logger.setLevel(logging.INFO)


class FooReceiver(AzureReceiver):
    def handle_body(self, body, **kwargs):
        instance_name = kwargs.pop('instance_name', '')
        instance_id = kwargs.pop('instance_id', '')
        print('topic: %s' % instance_name)
        print('mq received: %s' % body)

        foo_logger.info('AZURE-SB-FOO', extra=get_custom_properties(topic=instance_name, instance=f'{instance_name}-{instance_id}'))
