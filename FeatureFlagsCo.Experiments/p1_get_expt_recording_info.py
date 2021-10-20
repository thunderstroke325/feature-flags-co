from config.config_handling import get_config_value
import json
import os
import sys
import logging

from rabbitmq.rabbitmq import RabbitMQConsumer, RabbitMQSender

logger = logging.getLogger("p1_get_expt_recording_info")
logger.setLevel(logging.INFO)


class P1GetExptRecordingInfoConsumer(RabbitMQConsumer):

    def __setup_relation_between_obj_expt(self, dict_expt_id, key, expt_id):
        dict_from_redis = self.redis_get(dict_expt_id)
        dict_acitveExpts = dict_from_redis if dict_from_redis else {}
        list_act_Expts = dict_acitveExpts.get(key, [])
        list_act_Expts.append(expt_id)
        dict_acitveExpts[key] = list_act_Expts
        self.redis_set(dict_expt_id, dict_acitveExpts)

    def __check_format(self, body, key, fmt, *args):
        value = body.get(key, None)
        is_valid = False
        if value :
            if type(value) is fmt :
                if args:
                    if value in args[0] :
                        is_valid = True
                else:
                    is_valid = True
        return is_valid

    def handle_body(self, body, **properties):
        # Check Q1 format
        example_Q1_format = {
                                "ExptId":"exp1",
                                "IterationId":"1",
                                "EnvId":"111",
                                "FlagId":"FF__11__11__111__example",
                                "BaselineVariation":"1",
                                "Variations":["1","2","3"],
                                "EventName":"exampleName",
                                "EventType": 1,
                                "CustomEventTrackOption": 2,
                                "CustomEventSuccessCriteria": 1,
                                "CustomEventUnit": "Unit",
                                "StartExptTime":"2021-01-01T01:00:00.123456",
                                "EndExptTime":""
                            }
        check_pass = False
        if type(body) is dict:
            if example_Q1_format.keys() == body.keys():
                is_valid_ExptId = self.__check_format(body,'ExptId', str)
                is_valid_EventType= self.__check_format(body,'EventType', int, [1,2,3] )
                is_valid_CustomEventTrackOption= self.__check_format(body,'CustomEventTrackOption', int, [1,2] )
                is_valid_CustomEventSuccessCriteria= self.__check_format(body,'CustomEventSuccessCriteria', int, [1,2] )
                check_pass = ( is_valid_ExptId and is_valid_EventType and is_valid_CustomEventTrackOption and is_valid_CustomEventSuccessCriteria)
        if check_pass:
            key, end, value = body.get('ExptId', None), body.get(
                'EndExptTime', None), body
            logger.info('p1########p1 gets %r#########' % body)
            self.redis_set(key, value)
            if not end:
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
                self.send('Q2', 'py.experiments.experiment', key)
                logger.info('########p1 send %r to Q2########' % key)
        else :
            logger.info("ERROR: Invalid Q1 Format")



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
    P1GetExptRecordingInfoConsumer(
        mq_host, mq_port, mq_username, mq_passwd, redis_host, redis_port, redis_passwd).run('p1.experiments.consumer', ('Q1', ['py.experiments.recordinginfo.#']))
