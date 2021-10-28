#!/usr/bin/env python
import json
import logging

from rabbitmq.rabbitmq import RabbitMQSender

if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    sender1 = RabbitMQSender()
    sender2 = RabbitMQSender()
    json_body_1 = {'id': 123, 'value': 'event%s' % 123}
    json_body_2 = {'id': 456, 'value': 'event%s' % 456}
    sender1.redis.set(json_body_1['id'], str.encode(json.dumps(json_body_1)))
    sender2.redis.set(json_body_2['id'], str.encode(json.dumps(json_body_2)))
    sender1.send('topic1', "py.1", json_body_1)
    sender2.send('topic2', "py.2", json_body_2)
