import logging
from abc import ABC

from experiment.constants import (P3_FF_EVENT_NECESSAIRE_KEYS,
                                  P3_USER_EVENT_NECESSAIRE_KEYS)
from experiment.generic_sender_receiver import MessageHandler, RedisStub
from experiment.utils import encode, get_custom_properties


class P3GetExptFFEvents(RedisStub, MessageHandler, ABC):
    def handle_body(self, body, **kwargs):
        trace_logger = kwargs.pop('trace_logger', logging.getLogger(__name__))
        debug_logger = kwargs.pop('debug_logger', logging.getLogger(__name__))
        instance_name = kwargs.pop('instance_name', '')
        instance_id = kwargs.pop('instance_id', '')
        if type(body) is dict:
            missing_keys = [k for k in P3_FF_EVENT_NECESSAIRE_KEYS if k not in body.keys()]
            if not missing_keys:
                # not record an ff event which DOES not match any expt
                if self.redis.scard('dict_ff_act_expts_%s_%s' % (body['EnvId'], body['FeatureFlagId'])) > 0:
                    ff_env_id = '%s_%s' % (body['EnvId'], body['FeatureFlagId'])
                    dict_to_add = {
                        'UserKeyId': body['UserKeyId'],
                        'VariationLocalId': body['VariationLocalId'],
                        'TimeStamp': body['TimeStamp']
                    }
                    self.redis.rpush(ff_env_id, encode(dict_to_add))
                    debug_logger.info(f'topic: {instance_name}, FF EVENT: {dict_to_add}')
            else:
                trace_logger.warning('EVENT IGNOR', extra=get_custom_properties(topic=instance_name, instance=f'{instance_name}-{instance_id}', reason='UNVALID FORMAT', missing_keys=missing_keys))
        else:
            trace_logger.warning('EVENT IGNOR', extra=get_custom_properties(topic=instance_name, instance=f'{instance_name}-{instance_id}', reason='FORBIDDEN INPUT'))


class P3GetExptUserEvents(RedisStub, MessageHandler, ABC):
    def handle_body(self, body, **kwargs):
        trace_logger = kwargs.pop('trace_logger', logging.getLogger(__name__))
        debug_logger = kwargs.pop('debug_logger', logging.getLogger(__name__))
        instance_name = kwargs.pop('instance_name', '')
        instance_id = kwargs.pop('instance_id', '')
        if type(body) is dict:
            missing_keys = [k for k in P3_USER_EVENT_NECESSAIRE_KEYS if k not in body.keys()]
            if not missing_keys:
                # not record an user event which DOES not match any expt
                if self.redis.scard('dict_event_act_expts_%s_%s' % (body['EnvironmentId'], body['EventName'])) > 0:
                    event_env_id = '%s_%s' % (body['EnvironmentId'], body['EventName'])
                    dict_to_add = {
                        'UserKeyId': body['User']['FFUserKeyId'],
                        'NumericValue': body['NumericValue'],
                        'TimeStamp': body['TimeStamp']
                    }
                    self.redis.rpush(event_env_id, encode(dict_to_add))
                    debug_logger.info(f'topic: {instance_name}, USER EVENT: {dict_to_add}')
            else:
                trace_logger.warning('EVENT IGNOR', extra=get_custom_properties(topic=instance_name, instance=f'{instance_name}-{instance_id}', reason='UNVALID FORMAT', missing_keys=missing_keys))
        else:
            trace_logger.warning('EVENT IGNOR', extra=get_custom_properties(topic=instance_name, instance=f'{instance_name}-{instance_id}', reason='FORBIDDEN INPUT'))
