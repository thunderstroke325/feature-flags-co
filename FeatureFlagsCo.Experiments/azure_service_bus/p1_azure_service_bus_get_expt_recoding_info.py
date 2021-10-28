import logging

from azure_service_bus.constants import P1_NECESSAIRE_KEYS
from azure_service_bus.insight_utils import (get_custom_properties,
                                             get_insight_logger)
from azure_service_bus.send_consume import AzureReceiver
from azure_service_bus.utils import check_format
from config.config_handling import get_config_value

p1_logger = get_insight_logger('p1_azure_service_bus_get_expt_recoding_info')
p1_logger.setLevel(logging.INFO)


class P1AzureGetExptRecordingInfoReceiver(AzureReceiver):
    def handle_body(self, topic, body):
        if type(body) is dict:
            missing_keys = [k for k in P1_NECESSAIRE_KEYS if k not in body.keys()]
            if not missing_keys:
                key, end = body.get('ExptId', None), body.get('EndExptTime', None)
                if key:
                    # Deal with EndExptTime case
                    if end:
                        self.redis_set(key, body)
                        p1_logger.info('EXPT ENDING', extra=get_custom_properties(topic=topic, expt=key))
                    else:
                        # Continue format check
                        is_valid_EventType = check_format(body, 'EventType', int, [1, 2, 3])
                        is_valid_CustomEventTrackOption = check_format(body, 'CustomEventTrackOption', int, [1, 2])
                        is_valid_CustomEventSuccessCriteria = check_format(body, 'CustomEventSuccessCriteria', int, [1, 2])
                        check_latest_expt = (is_valid_EventType and is_valid_CustomEventTrackOption and is_valid_CustomEventSuccessCriteria)
                        self.redis_set(key, body)
                        # set up link between ff and his active expts
                        ff_env_id = 'dict_ff_act_expts_%s_%s' % (
                            body['EnvId'], body['FlagId'])
                        self.__setup_relation_between_obj_expt(
                            ff_env_id, body['FlagId'], key)
                        # set up link between event and his active expts
                        event_env_id = 'dict_event_act_expts_%s_%s' % (
                            body['EnvId'], body['EventName'])
                        self.__setup_relation_between_obj_expt(
                            event_env_id, body['EventName'], key)
                        topic = get_config_value('p2', 'topic_Q2')
                        subscription = get_config_value('p2', 'subscription_Q2')
                        self.send(self._bus, topic, subscription, key)
                        p1_logger.info('EXPT STARTING', extra=get_custom_properties(topic=topic, expt=key))
                        if not check_latest_expt:
                            p1_logger.warning('EXPT NOT RECOGNISED', extra=get_custom_properties(topic=topic,
                                                                                                 expt=key,
                                                                                                 reason='EXPT PARAMS INVALID',
                                                                                                 is_valid_EventType=is_valid_EventType,
                                                                                                 is_valid_CustomEventTrackOption=is_valid_CustomEventTrackOption,
                                                                                                 is_valid_CustomEventSuccessCriteria=is_valid_CustomEventSuccessCriteria))
                else:
                    p1_logger.warning('EXPT IGNOR', extra=get_custom_properties(topic=topic, expt=key, reason='EXPT NOT FOUND'))
            else:
                p1_logger.warning('EXPT IGNOR', extra=get_custom_properties(topic=topic, reason='UNVALID FORMAT', missing_keys=missing_keys))
        else:
            p1_logger.warning('EXPT IGNOR', extra=get_custom_properties(topic=topic, reason='FORBIDDEN INPUT'))

    def __setup_relation_between_obj_expt(self, dict_expt_id, key, expt_id):
        dict_acitveExpts = dict_from_redis if (
            dict_from_redis := self.redis_get(dict_expt_id)) else {}
        list_act_Expts = dict_acitveExpts.get(key, [])
        list_act_Expts.append(expt_id)
        dict_acitveExpts[key] = list_act_Expts
        self.redis_set(dict_expt_id, dict_acitveExpts)
