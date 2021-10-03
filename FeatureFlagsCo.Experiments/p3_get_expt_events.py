
from config.config_handling import get_config_value
import json
import logging
import os
import sys
from rabbitmq.rabbitmq import RabbitMQConsumer

logger = logging.getLogger("p3_get_expt_events")
logger.setLevel(logging.INFO)


class P3GetEventsConsumer(RabbitMQConsumer):

    def handle_body(self, body, **properties):
        routing_key = properties.get(
            'routing_key', None) if properties else None
        if type(body) is dict and routing_key:
            if 'py.experiments.events.ff' in routing_key:
                # Q4
                dict_flag_acitveExpts = self.redis_get(
                    'dict_ff_act_expts_%s_%s' % (body['EnvId'], body['FeatureFlagId']))
                if dict_flag_acitveExpts and dict_flag_acitveExpts.get(body['FeatureFlagId'], None):
                    id = '%s_%s' % (body['EnvId'], body['FeatureFlagId'])
                    value = self.redis_get(id)
                    list_ff_events = value if value else []
                    dict_to_add = {
                        # 'FeatureFlagId': body['FeatureFlagId'],
                        'UserKeyId': body['UserKeyId'],
                        'VariationLocalId': body['VariationLocalId'],
                        'TimeStamp': body['TimeStamp']
                    }
                    list_ff_events = list_ff_events + [dict_to_add]
                    self.redis_set(id, list_ff_events)
                    logger.info('Added ff event: %r' % dict_to_add)
            elif 'py.experiments.events.user' in routing_key:
                # Q5
                dict_customEvent_acitveExpts = self.redis_get(
                    'dict_event_act_expts_%s_%s' % (body['EnvironmentId'], body['EventName']))
                if dict_customEvent_acitveExpts and dict_customEvent_acitveExpts.get(body['EventName'], None):
                    id = '%s_%s' % (body['EnvironmentId'], body['EventName'])
                    value = self.redis_get(id)
                    list_user_events = value if value else []
                    dict_to_add = {
                        # 'EventName': body['EventName'],
                        'UserKeyId': body['User']['FFUserKeyId'],
                        'TimeStamp': body['TimeStamp']
                    }
                    list_user_events = list_user_events + [dict_to_add]
                    self.redis_set(id, list_user_events)
                    logger.info('Added user event: %r' % dict_to_add)


if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    mq_host = get_config_value('rabbitmq', 'mq_host')
    mq_port = get_config_value('rabbitmq', 'mq_port')
    mq_username = get_config_value('rabbitmq', 'mq_username')
    mq_passwd = get_config_value('rabbitmq', 'mq_passwd')
    redis_host = get_config_value('redis', 'redis_host')
    redis_port = get_config_value('redis', 'redis_port')
    redis_passwd = get_config_value('redis', 'redis_passwd')
    consumer = P3GetEventsConsumer(
        mq_host, mq_port, mq_username, mq_passwd, redis_host, redis_port, redis_passwd).run('p3.events.consumer', ('Q4', ['py.experiments.events.ff.#']), ('Q5', ['py.experiments.events.user.#']))
