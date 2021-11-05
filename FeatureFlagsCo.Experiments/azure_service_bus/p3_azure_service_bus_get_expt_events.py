import logging

from azure_service_bus.constants import (P3_FF_EVENT_NECESSAIRE_KEYS,
                                         P3_USER_EVENT_NECESSAIRE_KEYS)
from azure_service_bus.insight_utils import (get_custom_properties,
                                             get_insight_logger)
from azure_service_bus.send_consume import AzureReceiver
from azure_service_bus.utils import encode

p3_logger = get_insight_logger('trace_p3_azure_service_bus_get_expt_events')
p3_logger.setLevel(logging.INFO)

p3_debug_logger = logging.getLogger('debug_p3_azure_service_bus_get_expt_events')
p3_debug_logger.setLevel(logging.INFO)


class P3AzureGetExptFFEventsReceiver(AzureReceiver):
    def handle_body(self, instance_id, current_topic, body):
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
                    p3_debug_logger.info(f'topic: {current_topic}, FF EVENT: {dict_to_add}')
            else:
                p3_logger.warning('EVENT IGNOR', extra=get_custom_properties(topic=current_topic, instance=f'{current_topic}-{instance_id}', reason='UNVALID FORMAT', missing_keys=missing_keys))
        else:
            p3_logger.warning('EVENT IGNOR', extra=get_custom_properties(topic=current_topic, instance=f'{current_topic}-{instance_id}', reason='FORBIDDEN INPUT'))


class P3AzureGetExptUserEventsReceiver(AzureReceiver):
    def handle_body(self, instance_id, current_topic, body):
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
                    p3_debug_logger.info(f'topic: {current_topic}, USER EVENT: {dict_to_add}')
            else:
                p3_logger.warning('EVENT IGNOR', extra=get_custom_properties(topic=current_topic, instance=f'{current_topic}-{instance_id}', reason='UNVALID FORMAT', missing_keys=missing_keys))
        else:
            p3_logger.warning('EVENT IGNOR', extra=get_custom_properties(topic=current_topic, instance=f'{current_topic}-{instance_id}', reason='FORBIDDEN INPUT'))
