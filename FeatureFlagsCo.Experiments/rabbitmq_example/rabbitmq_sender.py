#!/usr/bin/env python
import json
import logging

import redis

from rabbitmq.rabbitmq import RabbitMQSender

if __name__ == '__main__':
    logging.basicConfig(encoding='utf-8', level=logging.INFO)
    r = redis.Redis(host='localhost', port=6379)
    sender = RabbitMQSender('localhost')
    routing_key = "es.a.b"
    jsons = []
    for mess in range(10):
        json_body = {'id': mess, 'value': 'hello world'}
        jsons.append(json_body)
        r.set(json_body['id'], str.encode(json.dumps(json_body)))
    sender.send('topic_ff', routing_key, *jsons)
