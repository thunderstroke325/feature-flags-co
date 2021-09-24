#!/usr/bin/env python
import json
import logging

from rabbitmq.rabbitmq import RabbitMQSender

if __name__ == '__main__':
    logging.basicConfig(encoding='utf-8', level=logging.INFO)
    sender = RabbitMQSender()
    routing_key = 'py.1'
    jsons = []
    for mess in range(10):
        json_body = {'id': mess, 'value': 'event%s' % mess}
        jsons.append(json_body)
        sender.redis.set(json_body['id'], str.encode(json.dumps(json_body)))
    sender.send('topic', routing_key, *jsons)
