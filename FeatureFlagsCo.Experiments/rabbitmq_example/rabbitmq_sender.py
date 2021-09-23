#!/usr/bin/env python
import logging

from rabbitmq.rabbitmq import RabbitMQSender

if __name__ == '__main__':
    logging.basicConfig(encoding='utf-8', level=logging.INFO)
    sender = RabbitMQSender('localhost')
    routing_key = "es.a.b"
    jsons = []
    for mess in range(10):
        json_body = {'id': mess, 'value': 'hello world'}
        jsons.append(json_body)
    sender.send('topic_ff', routing_key, *jsons)
