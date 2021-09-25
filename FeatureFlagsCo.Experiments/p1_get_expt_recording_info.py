import json
import os
import sys
import logging

from rabbitmq.rabbitmq import RabbitMQConsumer, RabbitMQSender


class P1GetExptRecordingInfoConsumer(RabbitMQConsumer):
    def handle_body(self, body):
        if type(body) is dict:
            key, end, value = body.get('ExptID', None), body.get(
                'EndExptTime', None), body
            if key:
                if end:
                    jsons = [key]
                    RabbitMQSender() \
                        .send(topic='Q2', routing_key='py.experiments.experimentresults', *jsons)
                self.redis.set(key, str.encode(json.dumps(value)))


if __name__ == '__main__':
    FORMAT = '%(asctime)-15s %(clientip)s %(user)-8s %(message)s'
    logging.basicConfig(format=FORMAT, encoding='utf-8', level=logging.INFO)
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
