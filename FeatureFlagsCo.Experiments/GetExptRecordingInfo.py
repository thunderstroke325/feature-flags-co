import json
import os
import sys

from rabbitmq.rabbitmq import RabbitMQConsumer, RabbitMQSender


class GetExptRecordingInfo(RabbitMQConsumer):
    def handle_body(self, body):
        if type(body) is dict:
            key, value = body.get('ExptID', None), body
            if key:
                jsons = [key]
                RabbitMQSender() \
                    .send(topic='Q2', routing_key='py.ExperimentResults', *jsons)
                self.redis.set(key, str.encode(json.dumps(value)))


if __name__ == '__main__':
    try:
        GetExptRecordingInfo().consumer(topic='Q1', queue='py.ExptRecordingInfo')
    except KeyboardInterrupt:
        print('Interrupted')
        try:
            sys.exit(0)
        except SystemExit:
            os._exit(0)
