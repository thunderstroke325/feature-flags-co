import logging

from azure_service_bus.insight_utils import (get_custom_properties,
                                             get_insight_logger)

from azure_service_bus.send_consume import AzureReceiver

foo_logger = get_insight_logger()
foo_logger.setLevel(logging.INFO)


class FooReceiver(AzureReceiver):
    def handle_body(self, topic, body):
        print('topic: %s' % topic)
        print('received: %s' % body)
        foo_logger.info('FOO', extra=get_custom_properties(topic=topic))
