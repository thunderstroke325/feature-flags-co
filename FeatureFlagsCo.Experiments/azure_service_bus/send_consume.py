import base64
import hashlib
import hmac
import logging
import time
from abc import ABC
from datetime import datetime, timedelta
from random import choice

import redis
from azure.servicebus import (AutoLockRenewer, ServiceBusClient,
                              ServiceBusMessage, ServiceBusReceiver,
                              ServiceBusSender)
from azure.servicebus._common.constants import ServiceBusSubQueue
from azure.servicebus.exceptions import (MessageAlreadySettled,
                                         MessageLockLostError,
                                         MessageNotFoundError,
                                         MessageSizeExceededError,
                                         ServiceBusError)
from config.config_handling import get_config_value
from experiment.constants import (ERROR_RETRY_INTERVAL, FMT,
                                  get_azure_instance_id)
from experiment.generic_sender_receiver import (MessageHandler, Receiver,
                                                RedisStub, Sender)
from experiment.utils import decode, encode, get_custom_properties, quite_app

from azure_service_bus.insight_utils import get_insight_logger

try:
    from urllib.parse import quote as url_parse_quote
except ImportError:
    from urllib import pathname2url as url_parse_quote

from azure.core.credentials import AzureSasCredential

debug_logger = logging.getLogger('debug_azure_sb_send_consume')
debug_logger.setLevel(logging.INFO)

engine = get_config_value('general', 'engine')
if engine == 'azure' or engine == 'redis':

    logger = get_insight_logger('trace_azure_sb_send_consume')
    logger.setLevel(logging.INFO)

    # The logging levels below may need to be changed based on the logging that you want to suppress.
    uamqp_logger = get_insight_logger('uamqp')
    uamqp_logger.setLevel(logging.ERROR)

    # or even further fine-grained control, suppressing the warnings in uamqp.connection module
    uamqp_connection_logger = get_insight_logger('uamqp.connection')
    uamqp_connection_logger.setLevel(logging.ERROR)

else:
    logger = logging.getLogger('trace_azure_sb_send_consume')


class AzureServiceBus(RedisStub):
    def __init__(self,
                 sb_host,
                 sb_sas_policy,
                 sb_sas_key,
                 redis_host='localhost',
                 redis_port=6379,
                 redis_passwd=''):
        self._sb_host = sb_host
        self._sb_sas_policy = sb_sas_policy
        self._sb_sas_key = sb_sas_key
        self._redis_host = redis_host
        self._redis_port = redis_port
        self._redis_passwd = redis_passwd
        try:
            self._ssl = True if int(redis_port) == 6380 else False
        except:
            self._ssl = False
        super()._init__redis_connection(redis_host, redis_port, redis_passwd, self._ssl)
        self._sender_pool = {}

    def clear(self):
        for _, sender in self._sender_pool.items():
            sender.close()
        self._sender_pool.clear()

    def _init_azure_service_bus(self, fully_qualified_namespace, sas_name, sas_value, token_ttl=timedelta(days=360)):
        """Performs the signing and encoding needed to generate a sas token from a sas key."""
        auth_uri = "sb://{}".format(fully_qualified_namespace)
        sas = sas_value.encode('utf-8')
        expiry = str(int(time.time() + token_ttl.total_seconds()))
        string_to_sign = (auth_uri + '\n' + expiry).encode('utf-8')
        signed_hmac_sha256 = hmac.HMAC(sas, string_to_sign, hashlib.sha256)
        signature = url_parse_quote(
            base64.b64encode(signed_hmac_sha256.digest()))
        sas_token = 'SharedAccessSignature sr={}&sig={}&se={}&skn={}'.format(
            auth_uri, signature, expiry, sas_name)
        credential = AzureSasCredential(sas_token)
        return ServiceBusClient(fully_qualified_namespace, credential, logging_enable=True)


class AzureSender(AzureServiceBus, Sender):

    # def send(self, bus: ServiceBusClient, topic: str, origin: str, *messages):
    def send(self, *messages, **kwargs):

        def send_batch_messages(sender: ServiceBusSender, topic, origin, *msgs):
            batch_message = sender.create_message_batch()
            for msg in msgs:
                try:
                    message = ServiceBusMessage(encode(msg), subject=topic, application_properties={'origin': origin})
                except TypeError:
                    # Message body is of an inappropriate type, must be string, bytes or None.
                    continue
                try:
                    batch_message.add_message(message)
                except MessageSizeExceededError:
                    # ServiceBusMessageBatch object reaches max_size.
                    # New ServiceBusMessageBatch object can be created here to send more data.
                    # This must be handled at the application layer, by breaking up or condensing.
                    continue
            last_error = None
            for _ in range(3):  # Send retries
                try:
                    sender.send_messages(batch_message)
                    last_error = None
                    return
                except MessageSizeExceededError:
                    # The body provided in the message to be sent is too large.
                    # This must be handled at the application layer, by breaking up or condensing.
                    logger.exception("The body provided in the message to be sent is too large")
                    break
                except ServiceBusError as e:
                    # Other types of service bus errors that can be handled at the higher level, such as connection/auth errors
                    # If it happens persistently, should bubble up, and should be logged/alerted if high volume.
                    last_error = e
                    continue
            if last_error:
                raise last_error
        bus = kwargs.pop('bus', None)
        topic = kwargs.pop('topic', '')
        subscription = kwargs.pop('subscription', '')
        if not bus:
            bus = self._init_azure_service_bus(
                self._sb_host, self._sb_sas_policy, self._sb_sas_key)
            with bus:
                if not (sender := self._sender_pool.get(topic, None)):
                    sender = bus.get_topic_sender(topic_name=topic)
                    self._sender_pool[topic] = sender
                send_batch_messages(sender, topic, subscription, *messages)
        else:
            if not (sender := self._sender_pool.get(topic, None)):
                sender = bus.get_topic_sender(topic_name=topic)
                self._sender_pool[topic] = sender
            send_batch_messages(sender, topic, subscription, *messages)
        debug_logger.info(f'send to topic: {topic}, origin: {subscription}, num of message: {len(messages)}')


class AzureReceiver(AzureSender, Receiver, MessageHandler, ABC):

    def consume(self, **kwargs):

        def receive_message(receiver: ServiceBusReceiver, process_name, topic=None, instance_id=None, renewer: AutoLockRenewer = None, settlement_retries=3, is_dlq=False):
            if is_dlq:
                debug_logger.info("################dlq receiver################")
            else:
                debug_logger.info("################normal receiver################")
            machine_id = get_azure_instance_id()
            should_retry = True
            while should_retry:
                try:
                    debug_logger.info(f'################pulling the message into {topic}-{instance_id}################')
                    if process_name:
                        current_pulling_timestamp = {'topic': topic, 'instance': instance_id, 'datetime': datetime.utcnow().strftime(FMT)}
                        self.redis.hset(f'topic_pulling_last_exec_time_in_{machine_id}', process_name, encode(current_pulling_timestamp))
                    for msg in receiver.receive_messages(max_message_count=None, max_wait_time=60):
                        last_error = None
                        try:
                            # Do your application-specific data processing here
                            self.handle_body(decode(str(msg)),
                                             bus=self._bus,
                                             instance_name=topic,
                                             instance_id=instance_id,
                                             trace_logger=logger,
                                             debug_logger=debug_logger)
                            should_complete = True
                        except ServiceBusError:
                            logger.exception("Maybe error in send message, retrying to connect...", extra=get_custom_properties(topic=msg.subject, instance=f'{msg.subject}-{instance_id}'))
                            raise
                        except redis.RedisError as e:
                            logger.exception('redis error', extra=get_custom_properties(topic=msg.subject, instance=f'{msg.subject}-{instance_id}'))
                            last_error = e
                            should_complete = False
                        except Exception as e:
                            logger.exception(f'Application level error in {msg.subject}', extra=get_custom_properties(topic=msg.subject, instance=f'{msg.subject}-{instance_id}'))
                            last_error = e
                            should_complete = False

                        for _ in range(settlement_retries):  # Settlement retry
                            try:
                                if renewer:
                                    renewer.register(receiver, msg, max_lock_renewal_duration=60)
                                if should_complete:
                                    receiver.complete_message(msg)
                                else:
                                    # Depending on the desired behavior, one could dead letter on failure instead; failure modes are comparable.
                                    # Abandon returns the message to the queue for another consumer to receive, dead letter moves to the dead letter subqueue.
                                    # maybe put the message into dead_letter_queue
                                    if isinstance(last_error, redis.RedisError):
                                        try:
                                            self.redis.ping()
                                        except:
                                            logger.exception('CANNOT PING redis, trying to reconnect...')
                                            self._init__redis_connection(self._redis_host,
                                                                         self._redis_port,
                                                                         self._redis_passwd,
                                                                         self._ssl)
                                        receiver.abandon_message(msg)
                                    elif is_dlq:
                                        # messages will be handled later
                                        # save them in redis
                                        id = f'{msg.subject}_py_deferred_sequenced_numbers'
                                        self.redis.sadd(id, str(msg.sequence_number))
                                        receiver.defer_message(msg)
                                    else:
                                        receiver.dead_letter_message(msg, reason=str(last_error), error_description='Application level failure')
                                break
                            except (MessageAlreadySettled, MessageLockLostError, MessageNotFoundError):
                                # Message was already settled, either somewhere earlier in this processing or by another node.  Continue.
                                # Message lock was lost before settlement.  Handle as necessary in the app layer for idempotency then continue on.
                                # Message has an improper sequence number, was dead lettered, or otherwise does not exist.  Handle at app layer, continue on.
                                logger.exception('message settled or lost or not found', extra=get_custom_properties(topic=msg.subject, instance=f'{msg.subject}-{instance_id}'))
                                break
                            except ServiceBusError:
                                # Any other undefined service errors during settlement.  Can be transient, and can retry, but should be logged, and alerted on high volume.
                                logger.exception('undefined service errors during settlement, retrying...', extra=get_custom_properties(topic=msg.subject, instance=f'{msg.subject}-{instance_id}'))
                                continue
                except ServiceBusError:
                    # some service bus error in connection that can be handled at the higher level, such as connection/auth errors
                    raise
                except BaseException as e:
                    # Although miscellaneous service errors and interruptions can occasionally occur during receiving,
                    # In most pragmatic cases one can try to continue receiving unless the failure mode seens persistent.
                    # Logging the associated failure and alerting on high volume is often prudent.
                    if isinstance(e, KeyboardInterrupt):
                        raise
                    logger.exception('service errors and interruptions occasionally occur during receiving, trying to fetch next message...')
                    continue

        process_name = kwargs.pop('process_name', '')
        topic = kwargs.pop('topic', '')
        connection_retries = kwargs.pop('connection_retries', 3)
        prefetch_count = kwargs.pop('prefetch_count', 10)
        settlement_retries = kwargs.pop('settlement_retries', 3)
        is_dlq = kwargs.pop('is_dlq', False)

        instance_id = choice([i for i in range(1000, 100000)])
        for _ in range(connection_retries):  # Connection retries.
            try:
                self._bus = self._init_azure_service_bus(self._sb_host, self._sb_sas_policy, self._sb_sas_key)
                debug_logger.info('################opening...################')
                with self._bus:
                    if topic:
                        topic_name, subscription = topic
                        if not is_dlq:
                            receiver = self._bus.get_subscription_receiver(topic_name=topic_name, subscription_name=subscription, prefetch_count=prefetch_count)
                        else:
                            receiver = self._bus.get_subscription_receiver(topic_name=topic_name,
                                                                           subscription_name=subscription,
                                                                           prefetch_count=prefetch_count,
                                                                           sub_queue=ServiceBusSubQueue.DEAD_LETTER)
                    else:
                        quite_app(0)
                    with AutoLockRenewer(max_workers=4) as renewer:
                        with receiver:
                            logger.info('RECEIVER START', extra=get_custom_properties(topic=topic_name, subscription=subscription, instance=f'{topic_name}-{instance_id}'))
                            receive_message(receiver, process_name, topic=topic_name, instance_id=instance_id, renewer=renewer, settlement_retries=settlement_retries, is_dlq=is_dlq)
            except ServiceBusError:
                logger.exception('An error occurred in service bus level, retrying to connect...', extra=get_custom_properties(
                    topic=topic_name, subscription=subscription, instance=f'{topic_name}-{instance_id}'))
                self.clear()
                time.sleep(ERROR_RETRY_INTERVAL)
                continue
            except KeyboardInterrupt:
                debug_logger.info('################Interrupted################')
                quite_app(0)
            except:
                logger.exception('unexpected error occurs, retrying to connect...', extra=get_custom_properties(topic=topic_name, subscription=subscription, instance=f'{topic_name}-{instance_id}'))
                continue

        logger.warning('APP QUIT', extra=get_custom_properties(topic=topic_name, subscription=subscription, instance=f'{topic_name}-{instance_id}', reason='TOO MANY RETRIES'))
        quite_app(1)
