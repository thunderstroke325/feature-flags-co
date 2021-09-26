import json
import os
import sys
import logging

from rabbitmq.rabbitmq import RabbitMQConsumer, RabbitMQSender


class P1GetExptRecordingInfoConsumer(RabbitMQConsumer):
    def handle_body(self, body, **properties):
        if type(body) is dict:
            key, end, value = body.get('ExptID', None), body.get(
                'EndExptTime', None), body
            if key:
                if not end:
                    jsons = [key]
                    RabbitMQSender() \
                        .send(topic='Q2', routing_key='py.experiments.experiment', *jsons)
                self.redis.set(key, str.encode(json.dumps(value)))


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    while True:
        try:
            P1GetExptRecordingInfoConsumer().consumer(
                'py.experiments.recordinginfo', ('Q1', []))
            break
        except KeyboardInterrupt:
            logging.info('#######Interrupted#########')
            try:
                sys.exit(0)
            except SystemExit:
                os._exit(0)
        except Exception as e:
            logging.exception("#######unexpected#########")
