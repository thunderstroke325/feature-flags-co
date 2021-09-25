#!/usr/bin/env python
import json
import logging

from rabbitmq.rabbitmq import RabbitMQSender

if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO,
                    format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                    datefmt='%m-%d %H:%M')
    sender = RabbitMQSender()
    routing_key = 'py.1'
    jsons = []
    for mess in range(10):
        json_body = {'id': mess, 'value': 'event%s' % mess}
        jsons.append(json_body)
        sender.redis.set(json_body['id'], str.encode(json.dumps(json_body)))
    sender.send('topic', routing_key, *jsons)
