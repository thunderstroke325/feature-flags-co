import logging
from datetime import datetime, timedelta
from time import sleep

from algo.expt_cal import calc_customevent_conversion, calc_customevent_numeric
from azure_service_bus.utils import decode
from config.config_handling import get_config_value

from azure_service_bus.insight_utils import (get_custom_properties,
                                             get_insight_logger)
from azure_service_bus.send_consume import AzureReceiver

p2_logger = get_insight_logger('trace_p2_azure_service_bus_get_expt_result')
p2_logger.setLevel(logging.INFO)

p2_debug_logger = logging.getLogger('debug_p2_azure_service_bus_get_expt_result')
p2_debug_logger.setLevel(logging.INFO)


class P2AzureGetExptResultReceiver(AzureReceiver):
    def __init__(self,
                 sb_host,
                 sb_sas_policy,
                 sb_sas_key,
                 redis_host='localhost',
                 redis_port='6379',
                 redis_passwd=None,
                 wait_timeout=30.0):
        super().__init__(sb_host, sb_sas_policy, sb_sas_key,
                         redis_host, redis_port, redis_passwd)
        self._last_expt_id = ''
        self._wait_timeout = wait_timeout

    def __parse_event_from_redis(self, expt, fmt):
        ExptStartTime = datetime.strptime(expt['StartExptTime'], fmt)
        ExptEndTime = None
        flag_id = expt['FlagId']
        event_name = expt['EventName']
        env_id = expt['EnvId']
        env_ff_id = '%s_%s' % (env_id, flag_id)
        env_event_id = '%s_%s' % (env_id, event_name)
        # Get list of events from Redis
        with self.redis.pipeline() as pipeline:
            pipeline.lrange(env_ff_id, 0, -1)
            pipeline.lrange(env_event_id, 0, -1)
            res = pipeline.execute()
        list_ff_events = [decode(ele) for ele in res[0]]
        list_user_events = [decode(ele) for ele in res[1]]
        # Filter Event according to Experiment StartTime
        list_ff_events = [ff_event for ff_event in list_ff_events if datetime.strptime(ff_event['TimeStamp'], fmt) >= ExptStartTime]
        list_user_events = [user_event for user_event in list_user_events if datetime.strptime(user_event['TimeStamp'], fmt) >= ExptStartTime]
        # Filter Event according to Experiment EndTime
        if expt['EndExptTime']:
            ExptEndTime = datetime.strptime(expt['EndExptTime'], fmt)
            list_ff_events = [ff_event for ff_event in list_ff_events if datetime.strptime(ff_event['TimeStamp'], fmt) <= ExptEndTime]
            list_user_events = [user_event for user_event in list_user_events if datetime.strptime(user_event['TimeStamp'], fmt) <= ExptEndTime]
        return expt, ExptStartTime, ExptEndTime, flag_id, event_name, env_id, list_ff_events, list_user_events

    def __update_redis_with_EndExpt(self, list_ff_events, list_user_events, fmt, ExptStartTime, ExptEndTime, expt_id, expt, current_topic='q2', instance_id=None):
        # Time to take decision to wait or not the upcomming event data
        para_delay_reception = 1
        para_wait_processing = 1
        # if empty list
        # If last event not received since N minutes, still wait M minutes after ExtpEndTime
        if list_ff_events and list_user_events:
            latest_ff_event_time = datetime.strptime(list_ff_events[-1]['TimeStamp'], fmt)
            latest_user_event_time = datetime.strptime(list_user_events[-1]['TimeStamp'], fmt)
            latest_event_end_time = max([latest_ff_event_time, latest_user_event_time])
        elif not list_ff_events and not list_user_events:
            latest_event_end_time = ExptStartTime
        else:
            latest_event_end_time = datetime.strptime((list_user_events + list_ff_events)[-1]['TimeStamp'], fmt)

        interval = ExptEndTime - latest_event_end_time
        # a delay to acept events after deadline
        if (interval > timedelta(minutes=para_delay_reception)) and ((datetime.now() - ExptEndTime) < timedelta(minutes=para_wait_processing)):
            # send back exptId to Q2
            topic = get_config_value('p2', 'topic_Q2')
            subscription = get_config_value('p2', 'subscription_Q2')
            self.send(self._bus, topic, subscription, expt_id)
            p2_debug_logger.info(f'a delay to acept events after deadline of {expt_id}')
        # last event received within N minutes, no potential recepton delay, proceed data deletion
        else:
            p2_debug_logger.info('Update info and delete stopped Experiment data')
            is_del_all_ff_events, is_del_all_user_events = False, False
            # ACTION : Update dict_flag_acitveExpts
            ff_act_expts_id = 'dict_ff_act_expts_%s_%s' % (expt['EnvId'], expt['FlagId'])
            self.redis.srem(ff_act_expts_id, expt_id)
            if self.redis.scard(ff_act_expts_id) == 0:
                is_del_all_ff_events = True

            # ACTION : Update dict_customEvent_acitveExpts
            event_act_expts_id = 'dict_event_act_expts_%s_%s' % (expt['EnvId'], expt['EventName'])
            self.redis.srem(event_act_expts_id, expt_id)
            if self.redis.scard(event_act_expts_id) == 0:
                is_del_all_user_events = True

            with self.redis.pipeline() as pipeline:
                # del expt
                ids = [expt_id]
                if is_del_all_ff_events:
                    # if ff is not related to any expt, remove ff events
                    ff_events_id = '%s_%s' % (expt['EnvId'], expt['FlagId'])
                    ids.append(ff_act_expts_id)
                    ids.append(ff_events_id)
                if is_del_all_user_events:
                    # if user is not related to any expt, remove user events
                    user_events_id = '%s_%s' % (expt['EnvId'], expt['EventName'])
                    ids.append(event_act_expts_id)
                    ids.append(user_events_id)
                pipeline.delete(*ids)
                # del expt expired time
                pipeline.hdel('dict_expt_last_exec_time', expt_id)
                pipeline.execute()
            self._last_expt_id = ''
            p2_logger.info('EXPT FINISH', extra=get_custom_properties(topic=current_topic, expt=expt_id, instance=f'{current_topic}-{instance_id}'))

    def handle_body(self, instance_id, current_topic, body):
        p2_logger.info('EXPT IN', extra=get_custom_properties(topic=current_topic, expt=body, instance=f'{current_topic}-{instance_id}'))
        starttime = datetime.now()
        expt_id = body
        value = decode(value) if (value := self.redis.get(expt_id)) else False
        fmt = '%Y-%m-%dT%H:%M:%S.%f'
        # Create or Get last_exec_time for each expt_id
        last_exec_time = last_exec_time if (last_exec_time := self.redis.hget('dict_expt_last_exec_time', expt_id)) else False
        interval_to_wait = 0
        if expt_id == self._last_expt_id:
            interval_to_wait = self._wait_timeout
        elif not last_exec_time:
            pass
        else:
            interval = datetime.now() - datetime.strptime(last_exec_time, fmt)
            if abs(interval.total_seconds()) < self._wait_timeout:
                interval_to_wait = self._wait_timeout - abs(interval.total_seconds())
        if interval_to_wait > 0 and interval_to_wait <= self._wait_timeout:
            p2_logger.info('EXPT SLEEP', extra=get_custom_properties(topic=current_topic, expt=expt_id, instance=f'{current_topic}-{instance_id}', sleep_time=str(interval_to_wait)))
            sleep(interval_to_wait)
        self.redis.hset('dict_expt_last_exec_time', expt_id, datetime.now().strftime(fmt))
        self._last_expt_id = expt_id
        p2_debug_logger.info(f'########p2 gets {value}#########')
        # If experiment info exist
        if value:
            # Parse experiment info
            expt, ExptStartTime, ExptEndTime, _, _, _, list_ff_events, list_user_events = self.__parse_event_from_redis(value, fmt)

            # Start To Calculate Experiment Result
            # call function to calculate experiment result
            # EventType: 1, 2 ,3 ; 分别是 customevent, pageview, click
            # CustomEventTrackOption: 1, 2 ; 分别是 conversion, numeric
            # CustomEventSuccessCriteria : 1, 2  for highest win and lowest win
            event_type, custom_event_track_option, is_compatible_mode = expt.get('EventType', None), expt.get('CustomEventTrackOption', None), False
            if event_type == 1:
                if custom_event_track_option == 1 :
                    output_to_mq = calc_customevent_conversion(expt, list_ff_events, list_user_events, logger=p2_debug_logger)
                elif custom_event_track_option == 2 :
                    output_to_mq = calc_customevent_numeric(expt, list_ff_events, list_user_events, logger=p2_debug_logger)
                else:
                    is_compatible_mode = True
            elif event_type == 2 or event_type == 3 :
                output_to_mq = calc_customevent_conversion(expt, list_ff_events, list_user_events, logger=p2_debug_logger)
            else:
                is_compatible_mode = True

            if is_compatible_mode:
                # Compatible with NON RECOGNISED experiments
                output_to_mq = calc_customevent_conversion(expt, list_ff_events, list_user_events, logger=p2_debug_logger)

            # send result to Q3
            s1 = datetime.now()
            topic = get_config_value('p2', 'topic_Q3')
            subscription = get_config_value('p2', 'subscription_Q3')
            self.send(self._bus, topic, subscription, output_to_mq)
            s2 = datetime.now()
            p2_debug_logger.info(f'#########expt result: {output_to_mq} to Q3 in seconds: {(s2-s1).total_seconds()}#########')
            # experiment not finished
            if not expt['EndExptTime']:
                # send back exptId to Q2
                s1 = datetime.now()
                topic = get_config_value('p2', 'topic_Q2')
                subscription = get_config_value('p2', 'subscription_Q2')
                self.send(self._bus, topic, subscription, expt_id)
                s2 = datetime.now()
                p2_debug_logger.info(f'#########expt: {expt_id} back to Q2 in seconds: {(s2-s1).total_seconds()}#########')
            else:
                # experiment has got its deadline
                # Decision to delete or not event related data
                self.__update_redis_with_EndExpt(list_ff_events, list_user_events, fmt, ExptStartTime, ExptEndTime, expt_id, expt, current_topic, instance_id)
            endtime = datetime.now()
            delta = endtime - starttime
            p2_debug_logger.info(f'#########p2 processing time in seconds: {delta.total_seconds()}#########')
            p2_logger.info('EXPT OUT', extra=get_custom_properties(topic=current_topic, expt=expt_id, instance=f'{current_topic}-{instance_id}'))
