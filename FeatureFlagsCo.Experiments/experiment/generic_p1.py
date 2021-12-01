
import logging
from abc import ABC

from config.config_handling import get_config_value

from experiment.constants import P1_NECESSAIRE_KEYS
from experiment.generic_sender_receiver import (MessageHandler, RedisStub,
                                                Sender)
from experiment.utils import check_format, encode, get_custom_properties


class P1GetExptRecordingInfo(RedisStub, Sender, MessageHandler, ABC):

    def __check_expt_status(self, expt):
        key, end = expt['ExptId'], expt.get('EndExptTime', False)
        exist_expt = self.redis.exists(key) == 1
        if not exist_expt and not end:
            return 's', ''
        elif exist_expt and not end:
            return 'i', 'EXPT START BUT EXPT EXIST'
        elif not exist_expt and end:
            return 'i', 'EXPT END BUT EXPT NOT EXIST'
        else:
            return 'e', ''

    def handle_body(self, body, **kwargs):
        logger = kwargs.pop('trace_logger', logging.getLogger(__name__))
        instance_name = kwargs.pop('instance_name', '')
        instance_id = kwargs.pop('instance_id', '')
        if type(body) is dict:
            missing_keys = [k for k in P1_NECESSAIRE_KEYS if k not in body.keys()]
            if not missing_keys:
                status, reson = self.__check_expt_status(body)
                if status == 'e':
                    # Deal with EndExptTime case
                    self.redis.set(body['ExptId'], encode(body))
                    logger.info('EXPT ENDING', extra=get_custom_properties(topic=instance_name, expt=body['ExptId'], instance=f'{instance_name}-{instance_id}'))
                elif status == 's':
                    # Continue check input format
                    is_valid_EventType = check_format(body, 'EventType', int, [1, 2, 3])
                    is_valid_CustomEventTrackOption = check_format(body, 'CustomEventTrackOption', int, [1, 2])
                    is_valid_CustomEventSuccessCriteria = check_format(body, 'CustomEventSuccessCriteria', int, [1, 2])
                    if not (is_valid_EventType and is_valid_CustomEventTrackOption and is_valid_CustomEventSuccessCriteria):
                        logger.warning('EXPT NOT RECOGNISED', extra=get_custom_properties(topic=instance_name,
                                                                                          expt=body['ExptId'],
                                                                                          instance=f'{instance_name}-{instance_id}',
                                                                                          reason='EXPT PARAMS INVALID',
                                                                                          is_valid_EventType=is_valid_EventType,
                                                                                          is_valid_CustomEventTrackOption=is_valid_CustomEventTrackOption,
                                                                                          is_valid_CustomEventSuccessCriteria=is_valid_CustomEventSuccessCriteria))
                    with self.redis.pipeline() as pipeline:
                        pipeline.set(body['ExptId'], encode(body))
                        # set up link between ff and his active expts
                        ff_env_id = 'dict_ff_act_expts_%s_%s' % (body['EnvId'], body['FlagId'])
                        pipeline.sadd(ff_env_id, body['ExptId'])
                        # set up link between event and his active expts
                        event_env_id = 'dict_event_act_expts_%s_%s' % (body['EnvId'], body['EventName'])
                        pipeline.sadd(event_env_id, body['ExptId'])
                        pipeline.execute()
                    kwargs['topic'] = get_config_value('p2', 'topic_Q2')
                    kwargs['subscription'] = get_config_value('p2', 'subscription_Q2')
                    self.send(body['ExptId'], **kwargs)
                    logger.info('EXPT STARTING', extra=get_custom_properties(topic=instance_name, expt=body['ExptId'], instance=f'{instance_name}-{instance_id}'))

                else:
                    logger.warning('EXPT IGNOR', extra=get_custom_properties(topic=instance_name, expt=body['ExptId'], instance=f'{instance_name}-{instance_id}', reason=reson))
            else:
                logger.warning('EXPT IGNOR', extra=get_custom_properties(topic=instance_name, instance=f'{instance_name}-{instance_id}', reason='UNVALID FORMAT', missing_keys=missing_keys))
        else:
            logger.warning('EXPT IGNOR', extra=get_custom_properties(topic=instance_name, instance=f'{instance_name}-{instance_id}', reason='FORBIDDEN INPUT'))
