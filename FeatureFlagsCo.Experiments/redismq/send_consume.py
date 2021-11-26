import logging
from abc import ABC
from datetime import datetime
from random import choice
from time import sleep

import redis
from azure_service_bus.insight_utils import get_insight_logger
from config.config_handling import get_config_value
from experiment.constants import (ERROR_RETRY_INTERVAL, FMT, SYS_HEARTBEAT,
                                  get_azure_instance_id)
from experiment.generic_sender_receiver import (MessageHandler, Receiver,
                                                RedisStub, Sender)
from experiment.utils import decode, encode, get_custom_properties, quite_app

engine = get_config_value('general', 'engine')
if engine == 'azure' or engine == 'redis':
    logger = get_insight_logger('trace_redis_send_consume')
else:
    logger = logging.getLogger('trace_redis_send_consume')
logger.setLevel(logging.INFO)

debug_logger = logging.getLogger('debug_redis_send_consume')
debug_logger.setLevel(logging.INFO)


class RedisSender(RedisStub, Sender):
    def send(self, *messages, **kwargs):
        topic = kwargs.pop('topic', '')
        last_error = None
        for _ in range(3):  # Send retries
            try:
                with self.redis.pipeline() as pipeline:
                    for msg in messages:
                        pipeline.rpush(topic, encode(msg))
                    pipeline.execute()
                debug_logger.info(f'send to topic: {topic}, num of message: {len(messages)}')
                return
            except Exception as e:
                last_error = e
        if last_error:
            raise last_error


class RedisReceiver(RedisSender, Receiver, MessageHandler, ABC):

    def consume(self, **kwargs):
        process_name = kwargs.pop('process_name', '')
        topic = kwargs.pop('topic', '')
        health_check_interval = kwargs.pop('health_check_interval', 30)
        instance_id = choice([i for i in range(1000, 100000)])
        machine_id = get_azure_instance_id()
        is_exit_in_error = False
        message = None

        logger.info('RECEIVER START', extra=get_custom_properties(topic=topic, instance=f'{topic}-{instance_id}'))
        while True:
            try:
                debug_logger.info(f'################pulling the message into {topic}-{instance_id}################')
                self.__health_check_reponse(process_name, machine_id, topic, instance_id)
                item = item if (item := self.redis.blpop(topic, timeout=health_check_interval)) else None
                if item:
                    message = decode(item[1])
                    self.handle_body(message,
                                     instance_name=topic,
                                     instance_id=instance_id,
                                     trace_logger=logger,
                                     debug_logger=debug_logger)
                sleep(SYS_HEARTBEAT)
            except redis.RedisError:
                logger.exception('redis error', extra=get_custom_properties(topic=topic, instance=f'{topic}-{instance_id}'))
                try:
                    self.redis.ping()
                    if message:
                        self.send(message, topic=topic)
                    sleep(ERROR_RETRY_INTERVAL)
                except:
                    logger.exception('redis conn lost, ready to restart, maybe data lost', extra=get_custom_properties(topic=topic, instance=f'{topic}-{instance_id}'))
                    # TODO back up last unhandled message in some places
                    is_exit_in_error = True
                    break
            except KeyboardInterrupt:
                debug_logger.info('################Interrupted################')
                break
            except:
                logger.exception('unexpected error occurs', extra=get_custom_properties(topic=topic, instance=f'{topic}-{instance_id}'))
                if message:
                    self.send(message, topic=f'dead_letters_{topic}')

        if is_exit_in_error:
            quite_app(1)
        else:
            quite_app(0)

    def __health_check_reponse(self, process_name, machine_id, topic, instance_id):
        if process_name:
            current_pulling_timestamp = {'topic': topic, 'instance': instance_id, 'datetime': datetime.utcnow().strftime(FMT)}
            self.redis.hset(f'topic_pulling_last_exec_time_in_{machine_id}', process_name, encode(current_pulling_timestamp))
