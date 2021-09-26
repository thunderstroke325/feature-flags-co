
import json
import logging
import os
import sys
from rabbitmq.rabbitmq import RabbitMQConsumer


class P3GetEventsConsumer(RabbitMQConsumer):

    def handle_body(self, body, **properties):
        routing_key = properties.get(
            'routing_key', None) if properties else None
        if type(body) is dict and routing_key:
            if routing_key.contains('py.experiments.events.ff') and body['_index'] == 'ffvariationrequestindex':
                # Q4
                dict_flag_acitveExpts = json.loads(self.redis.get(
                    'dict_ff_act_expts_%s_%s' % (body['EnvId'], body['FeatureFlagId'])).decode())
                if body['FeatureFlagId'] in dict_flag_acitveExpts.keys():
                    id = '%s_%s' % (body['EnvId'], body['FeatureFlagId'])
                    value = self.redis.get(id)
                    list_ff_events = json.loads(
                        value.decode()) if value else []
                    dict_to_add = {
                        # 'FeatureFlagId': body['FeatureFlagId'],
                        'UserKeyId': body['UserKeyId'],
                        'VariationLocalId': body['VariationLocalId'],
                        'TimeStamp': body['TimeStamp']
                    }
                    list_ff_events = list_ff_events + [dict_to_add]
                    self.redis.set(id, str.encode(json.dumps(list_ff_events)))
            elif routing_key.contains('py.experiments.events.user') and body['_index'] == 'experiments':
                # Q5
                dict_customEvent_acitveExpts = json.loads(self.redis.get(
                    'dict_event_act_expts_%s_%s' % (body['EnvironmentId'], body['EventName'])).decode())
                if body["EventName"] in dict_customEvent_acitveExpts.keys():
                    id = '%s_%s' % (body['EnvironmentId'], body['EventName'])
                    value = self.redis.get(id)
                    list_user_events = json.loads(
                        value.decode()) if value else []
                    dict_to_add = {
                        # "EventName": body["EventName"],
                        'UserKeyI': body["User"]["FFUserKeyId"],
                        'TimeStamp': body["TimeStamp"]
                    }
                    list_user_events = list_user_events + [dict_to_add]
                    self.redis.set(id, str.encode(
                        json.dumps(list_user_events)))


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    while True:
        try:
            P3GetEventsConsumer().consumer(
                '', ('Q4', ['py.experiments.events.ff.#']), ('Q5', ['py.experiments.events.user.#']))
            break
        except KeyboardInterrupt:
            logging.info('#######Interrupted#########')
            try:
                sys.exit(0)
            except SystemExit:
                os._exit(0)
        except Exception as e:
            logging.exception("#######unexpected#########")
