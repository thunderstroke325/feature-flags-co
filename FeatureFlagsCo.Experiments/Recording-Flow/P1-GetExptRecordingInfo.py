#!/usr/bin/env python
import pika, sys, os
import pickle
sys.path.append('../')
from config.config_handling import get_config_value
from requests.auth import HTTPBasicAuth 
import json

_mq_host = get_config_value('rabbitmq', 'mq_host')
_mq_port = get_config_value('rabbitmqport', 'mq_port')
_mq_username= get_config_value('rabbitmqusername', 'mq_username')
_mq_passwd = get_config_value('rabbitmqpasswd', 'mq_passwd')


def GetExptRecordingInfo():
    credentials = pika.PlainCredentials(_mq_username, _mq_passwd)
    parameters = pika.ConnectionParameters(_mq_host,
                                    _mq_port,
                                    '/',
                                    credentials)
    connection = pika.BlockingConnection(parameters)
    channel = connection.channel()
    queue_name = 'ExptRecordingInfo'
    channel.queue_declare(queue=queue_name)

    def callback(ch, method, properties, body):
        body_message = pickle.loads(body)
        print(" [x] Received %r" % body_message)
        return body_message

    channel.basic_consume(queue=queue_name, on_message_callback=callback, auto_ack=True)

    print(' [*] Waiting for messages. To exit press CTRL+C')
    channel.start_consuming()


if __name__ == '__main__':
    try:
        GetExptRecordingInfo()

    except KeyboardInterrupt:
        print('Interrupted')
        try:
            sys.exit(0)
        except SystemExit:
            os._exit(0)