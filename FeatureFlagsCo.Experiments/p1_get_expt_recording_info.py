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

    def handle_body(self, body, **properties):
        if type(body) is dict:
            key, end, value = body.get('ExptId', None), body.get(
                'EndExptTime', None), body
            if key:
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
