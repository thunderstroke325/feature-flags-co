import json
import logging
import os
import sys
import numpy as np
import scipy as sp
from scipy import stats
from datetime import datetime
import math
import pandas as pd
from rabbitmq.rabbitmq import RabbitMQConsumer, RabbitMQSender


class P2GetExptResult(RabbitMQConsumer):

    # cal Confidence interval
    def __mean_confidence_interval(data, confidence=0.95):
        a = 1.0 * np.array(data)
        n = len(a)
        m, se = np.mean(a), sp.stats.sem(a)
        h = se * sp.stats.t.ppf((1 + confidence) / 2., n-1)
        return m, m-h, m+h

    def handle_body(self, body, **properties):
        starttime = datetime.now()
        expt_id = body
        value = self.redis.get(expt_id)
        fmt='%y-%m-%dT%H:%M:%S.%f'
        if value:
            expt = json.loads(value.decode())
            flag_id = expt["Flag"]["Id"]
            event_name = expt["EventName"]
            env_id = expt["EnvId"]
            env_ff_id = '%s_%s' % (env_id, flag_id)
            env_event_id = '%s_%s' % (env_id, event_name)
            value_events_ff = self.redis.get(env_ff_id)
            list_ff_events = json.loads(
                value_events_ff.decode()) if value_events_ff else []
            value_user_events = self.redis.get(env_event_id)
            list_user_events = json.loads(
                value_user_events.decode()) if value_user_events else []
            list_ff_events = [ff_event for ff_event in list_ff_events if datetime.strptime(
                ff_event['TimeStamp'], fmt) >= datetime.strptime(expt['StartExptTime'], fmt)]
            list_user_events = [user_event for user_event in list_user_events if datetime.strptime(
                user_event['TimeStamp'], fmt) >= datetime.strptime(expt['StartExptTime'], fmt)]
            if expt["EndExptTime"]:
                list_ff_events = [ff_event for ff_event in list_ff_events if datetime.strptime(
                    ff_event['TimeStamp'], fmt) <= datetime.strptime(expt['EndExptTime'], fmt)]
                list_user_events = [user_event for user_event in list_user_events if datetime.strptime(
                    user_event['TimeStamp'], fmt) <= datetime.strptime(expt['EndExptTime'], fmt)]
                latest_ff_event_time = datetime.strptime(list_ff_events[-1]['TimeStamp'], fmt)
                latest_user_event_time = datetime.strptime(list_user_events[-1]['TimeStamp'], fmt)
                latest_event_end_time = max([latest_ff_event_time, latest_user_event_time])
                interval = datetime.strptime(expt["EndExptTime"], fmt) - latest_event_end_time
                ########## ? if interval >
            # expt not active
            if expt["EndExptTime"] and (not list_ff_events or not list_user_events):
                # Update dict_flag_acitveExpts
                # ACTION : Get from Redis > dict_flag_acitveExpts
                id = 'dict_ff_act_expts_%s_%s' % (
                    expt['EnvId'], expt['FeatureFlagId'])
                dict_flag_acitveExpts = json.loads(self.redis.get(id).decode())
                dict_flag_acitveExpts[expt['FeatureFlagId']].remove(
                    expt['ExptId'])
                self.redis.set(id, str.encode(
                    json.dumps(dict_flag_acitveExpts)))
                # Update dict_flag_acitveExpts
                # ACTION : Get from Redis > dict_flag_acitveExpts
                id = 'dict_ff_act_expts_%s_%s' % (
                    expt['EnvId'], expt['EventName'])
                dict_customEvent_acitveExpts = json.loads(
                    self.redis.get(id).decode())
                dict_customEvent_acitveExpts[expt['EventName']].remove(
                    expt['ExptId'])
                self.redis.set(id, str.encode(
                    json.dumps(dict_customEvent_acitveExpts)))
                # ACTION: Delete in Redis > list_FFevent related to FlagID
                # ACTION: Delete in Redis > list_Exptevent related to EventName
                logging.info('Update info and delete stopped Experiment data')
                if not dict_flag_acitveExpts.get(expt['FeatureFlagId'], None):
                    id = '%s_%s' % (expt['EnvId'], expt['FeatureFlagId'])
                    self.redis.delete(id)
                    # TODO move to somewhere
                if not dict_customEvent_acitveExpts.get(expt['Event'], None):
                    id = '%s_%s' % (expt['EnvId'], expt['EventName'])
                    self.redis.delete(id)
                    # TODO move to somewhere

            else:
                if list_ff_events and list_user_events:
                    df_ff_events = pd.DataFrame(list_ff_events)
                    ff_events_agg = df_ff_events.sort_values('TimeStamp').groupby(
                        "UserKeyId").last().reset_index().to_dict(orient='record')

                    df_user_events = pd.DataFrame(list_user_events)
                    df_user_events = df_user_events.sort_values('TimeStamp').groupby(
                        "UserKeyId").last().reset_index().to_dict(orient='record')

                    # Stat of Flag
                    var_baseline = expt['Flag']['BaselineVariation']
                    dict_var_user = {}
                    dict_var_occurence = {}

                    for item in ff_events_agg:
                        value = item['VariationLocalId']
                        user = item['UserKeyId']
                        if value not in list(dict_var_occurence.keys()):
                            dict_var_occurence[value] = 1
                            dict_var_user[value] = [user]
                        else:
                            dict_var_occurence[value] = dict_var_occurence[value] + 1
                            dict_var_user[value] = dict_var_user[value] + [user]

                    logging.info('dictionary of flag var:occurence')
                    logging.info(dict_var_occurence)
                    for item in dict_var_user.keys():
                        dict_var_user[item] = list(set(dict_var_user[item]))

                    # Stat of Expt.
                    dict_expt_occurence = {}
                    for item in list_ff_events:
                        user = item['UserKeyId']
                        for it in dict_var_user.keys():
                            if user in dict_var_user[it]:
                                if it not in list(dict_expt_occurence.keys()):
                                    dict_expt_occurence[it] = 1
                                else:
                                    dict_expt_occurence[it] = 1 + \
                                        dict_expt_occurence[it]

                    logging.info('dictionary of expt var:occurence')
                    logging.info(dict_expt_occurence)
                    output = []
                    # If  (baseline variation usage = 0) or (baseline variation customer event = 0 )
                    if var_baseline not in list(dict_var_occurence.keys()) or var_baseline not in dict_expt_occurence.keys():
                        for item in dict_var_occurence.keys():
                            if item in dict_expt_occurence.keys():
                                dist_item = [1 for i in range(dict_expt_occurence[item])] + [
                                    0 for i in range(dict_var_occurence[item]-dict_expt_occurence[item])]
                                rate, min, max = self.__mean_confidence_interval(
                                    dist_item)
                                if math.isnan(min) or math.isnan(max):
                                    confidenceInterval = [-1, -1]
                                else:
                                    confidenceInterval = [0 if round(min, 2) < 0 else round(
                                        min, 2), 1 if round(max, 2) > 1 else round(max, 2)]
                                pValue = -1
                                output.append({'variation': item,
                                               'conversion': dict_expt_occurence[item],
                                               'uniqueUsers': dict_var_occurence[item],
                                               'conversionRate':   round(rate, 2),
                                               'changeToBaseline': -1,
                                               'confidenceInterval': confidenceInterval,
                                               'pValue': -1,
                                               'isBaseline': True if expt['Flag']['BaselineVariation'] == item else False,
                                               'isWinner': False,
                                               'isInvalid': True
                                               }
                                              )
                    else:
                        BaselineRate = dict_expt_occurence[var_baseline] / \
                            dict_var_occurence[var_baseline]
                        # Preprare Baseline data sample distribution for Pvalue Calculation
                        dist_baseline = [1 for i in range(dict_expt_occurence[var_baseline])] + [
                            0 for i in range(dict_var_occurence[var_baseline]-dict_expt_occurence[var_baseline])]
                        for item in dict_var_occurence.keys():
                            if item in dict_expt_occurence.keys():
                                dist_item = [1 for i in range(dict_expt_occurence[item])] + [
                                    0 for i in range(dict_var_occurence[item]-dict_expt_occurence[item])]
                                rate, min, max = self.__mean_confidence_interval(
                                    dist_item)
                                if math.isnan(min) or math.isnan(max):
                                    confidenceInterval = [-1, -1]
                                else:
                                    confidenceInterval = [0 if round(min, 2) < 0 else round(
                                        min, 2), 1 if round(max, 2) > 1 else round(max, 2)]
                                pValue = round(
                                    1-stats.ttest_ind(dist_baseline, dist_item).pvalue, 2)
                                output.append({'variation': item,
                                               'conversion': dict_expt_occurence[item],
                                               'uniqueUsers': dict_var_occurence[item],
                                               'conversionRate':   round(rate, 3),
                                               'changeToBaseline': round(rate, 3) - round(BaselineRate, 3),
                                               'confidenceInterval': confidenceInterval,
                                               'pValue': -1 if math.isnan(pValue) else pValue,
                                               'isBaseline': True if expt['Flag']['BaselineVariation'] == item else False,
                                               'isWinner': False,
                                               'isInvalid': True if (pValue < 0.95) or math.isnan(pValue) else False
                                               }
                                              )
                        # Get winner variation
                        listValid = [output.index(
                            item) for item in output if item['isInvalid'] == False]

                        # If at least one variation is valid:
                        if len(listValid) != 0:
                            dictValid = {}
                            for index in listValid:
                                dictValid[index] = output[index]['conversionRate']
                            maxRateIndex = [k for k, v in sorted(
                                dictValid.items(), key=lambda item: item[1])][-1]
                            # when baseline has the highest conversion rate
                            if output[maxRateIndex]['changeToBaseline'] > 0:
                                output[maxRateIndex]['isWinner'] = True

                    print('ExptResults:')
                    print(output)

                    output_to_mq = {
                        "ExperimentId": expt_id,
                        "StartTime": expt["StartExptTime"],
                        "EndTime": datetime.now(),
                        "Results": output
                    }

                    RabbitMQSender() \
                        .send(topic='Q3', routing_key='py.experiments.experiment.results', *[output_to_mq])

                    RabbitMQSender() \
                        .send(topic='Q2', routing_key='py.experiments.experiment', *[expt_id])
                    logging.info('send back to Q2 ExptID')

                    endtime = datetime.now()
                    logging.info('processing time:')
                    logging.info((endtime-starttime))


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    while True:
        try:
            P2GetExptResult().consumer('py.experiments.experiment', ('Q2', []))
            break
        except KeyboardInterrupt:
            logging.info('#######Interrupted#########')
            try:
                sys.exit(0)
            except SystemExit:
                os._exit(0)
        except Exception as e:
            logging.exception("#######unexpected#########")
