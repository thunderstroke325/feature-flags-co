import logging
import math
from datetime import datetime
from math import ceil

import numpy as np
import pandas as pd
import scipy as sp
from scipy import stats
from statsmodels.stats.power import TTestIndPower, tt_ind_solve_power, NormalIndPower, zt_ind_solve_power
from statsmodels.stats.proportion import proportions_ztest

# cal Confidence interval
def mean_confidence_interval(data, confidence=0.95):
    a = 1.0 * np.array(data)
    n = len(a)
    m, se = np.mean(a), sp.stats.sem(a)
    h = se * sp.stats.t.ppf((1 + confidence) / 2., n - 1)
    return m, m - h, m + h


# cal Expt Result from list of FlagsEvents and list of CustomEvents
def calc_customevent_conversion(expt, list_ff_events, list_user_events, logger=logging.getLogger(__name__)):
    # Set power and alpha default value for expt.
    para_power = 0.8
    para_alpha = 0.05
    ratio_power = 1.0
    # User's flags event aggregation, if not empty
    if list_ff_events:
        df_ff_events = pd.DataFrame(list_ff_events)
        ff_events_agg = df_ff_events.sort_values('TimeStamp').groupby('UserKeyId').last().reset_index().to_dict('records')
    else:
        ff_events_agg = []
    # User's custom event aggregation, if not empty
    if list_user_events:
        df_user_events = pd.DataFrame(list_user_events)
        user_events_agg = df_user_events.sort_values('TimeStamp').groupby('UserKeyId').last().reset_index().to_dict('records')
    else:
        user_events_agg = []
    # Stat of Flag
    var_baseline = expt['BaselineVariation']
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
    logger.info('dictionary of flag var:occurence')
    logger.info(dict_var_occurence)

    for item in dict_var_user.keys():
        dict_var_user[item] = list(set(dict_var_user[item]))

    dict_expt_occurence = {}
    for item in user_events_agg:
        user = item['UserKeyId']
        for it in dict_var_user.keys():
            if user in dict_var_user[it]:
                if it not in list(dict_expt_occurence.keys()):
                    dict_expt_occurence[it] = 1
                else:
                    dict_expt_occurence[it] = 1 + dict_expt_occurence[it]
    logger.info('dictionary of expt var:occurence')
    logger.info(dict_expt_occurence)

    # Get User Power & ExperimentEffect Expections
    # If not given, Use Power=0.8 as default value
    Compare_Power = False
    if not expt.get('Power', None):
        # Compare power value with 0.8 to decide isInvalid
        Power = para_power
        Compare_Power = True
    else:
        # Compare sample with minimum sample calculated
        Power = expt['Power']
        ExpectedExperimentEffect = expt['ExpectedExperimentEffect']

    # list of results by flag-variation
    output = []
    for var in expt['Variations']:
        if var not in dict_var_occurence.keys():
            output.append({'variation': var,
                           'conversion': None,
                           'uniqueUsers': None,
                           'conversionRate': None,
                           'changeToBaseline': None,
                           'confidenceInterval': None,
                           'pValue': None,
                           'isBaseline': True if var_baseline == var else False,
                           'isWinner': False,
                           'isInvalid': True
                           })
    # If  (baseline variation usage = 0) or (baseline variation customer event = 0 )
    if var_baseline not in list(dict_var_occurence.keys()) or var_baseline not in dict_expt_occurence.keys():
        # output only the flag-variation used.
        for item in dict_var_occurence.keys():
            if item in dict_expt_occurence.keys():
                dist_item = [1 for i in range(dict_expt_occurence[item])] + [0 for i in range(dict_var_occurence[item] - dict_expt_occurence[item])]
                rate, min, max = mean_confidence_interval(dist_item, 1 - para_alpha)
                if math.isnan(min) or math.isnan(max):
                    confidenceInterval = None
                else:
                    confidenceInterval = [0 if round(min, 3) < 0 else round(min, 3), 1 if round(max, 3) > 1 else round(max, 3)]
                pValue = None
                output.append({'variation': item,
                               'conversion': dict_expt_occurence[item],
                               'uniqueUsers': dict_var_occurence[item],
                               'conversionRate':round(rate, 3),
                               'changeToBaseline': None,
                               'confidenceInterval': confidenceInterval,
                               'pValue': pValue,
                               'isBaseline': True if var_baseline == item else False,
                               'isWinner': False,
                               'isInvalid': True
                               })
            else:
                output.append({'variation': item,
                               'conversion': 0,
                               'uniqueUsers': dict_var_occurence[item],
                               'conversionRate': 0,
                               'changeToBaseline': None,
                               'confidenceInterval': None,
                               'pValue': None,
                               'isBaseline': True if var_baseline == item else False,
                               'isWinner': False,
                               'isInvalid': True
                               })
    else:
        BaselineRate = dict_expt_occurence[var_baseline] / dict_var_occurence[var_baseline]
        # Preprare Baseline data sample distribution for Pvalue Calculation
        dist_baseline = [1 for i in range(dict_expt_occurence[var_baseline])] + [0 for i in range(dict_var_occurence[var_baseline] - dict_expt_occurence[var_baseline])]
        for item in dict_var_occurence.keys():
            if item in dict_expt_occurence.keys():
                dist_item = [1 for i in range(dict_expt_occurence[item])] + [0 for i in range(dict_var_occurence[item] - dict_expt_occurence[item])]
                rate, min, max = mean_confidence_interval(dist_item, 1 - para_alpha)
                if math.isnan(min) or math.isnan(max):
                    confidenceInterval = None
                else:
                    confidenceInterval =  [0 if round(min, 2) < 0 else round(min, 2), 1 if round(max, 2) > 1 else round(max, 2)]
                # Calculate pValue
                pValue = round(proportions_ztest(sum(dist_item), len(dist_item), alternative='two-sided', prop_var=False)[1], 2)
                # Calculate Power
                try:
                    s1 = int(dict_var_occurence[var_baseline])
                    s2 = int(dict_var_occurence[item])
                    sampleNow = s1 if s1 < s2 else s2
                    if Compare_Power:
                        # calculate power compared to default power value
                        Effect = (rate - BaselineRate) / BaselineRate
                        power_now = NormalIndPower.power(Effect, sampleNow, para_alpha)
                        if power_now > para_power:
                            power_valid = True
                        else:
                            power_valid = False
                        logger.info(f'calculate power compared to default value: {power_now}')
                    else:
                        # calculate miminum sample
                        required_n = ceil(zt_ind_solve_power(
                            ExpectedExperimentEffect,
                            power=Power,
                            alpha=para_alpha,
                            ratio=ratio_power)
                        )
                        if sampleNow > required_n:
                            power_valid = True
                        else:
                            power_valid = False
                        logger.info(f'calculate minimum sample size: {required_n}')
                except:
                    logger.info('ERROR in power calculation, return power_valid False')
                    power_valid = False

                if (pValue.value > para_alpha) or math.isnan(pValue.value):
                    pValue_valid = False
                else:
                    pValue_valid = True
                if pValue_valid and power_valid :
                    isInvalid = False
                else:
                    isInvalid = True
                    
                if math.isnan(pValue.value):
                    pValue = None
                                        
                output.append({'variation': item,
                               'conversion': dict_expt_occurence[item],
                               'uniqueUsers': dict_var_occurence[item],
                               'conversionRate': round(rate, 3),
                               'changeToBaseline': round(rate, 3) - round(BaselineRate, 3),
                               'confidenceInterval': confidenceInterval,
                               'pValue': pValue,
                               'isBaseline': True if var_baseline == item else False,
                               'isWinner': False,
                               'isInvalid': isInvalid
                               })
            else:
                output.append({'variation': item,
                               'conversion': 0,
                               'uniqueUsers': dict_var_occurence[item],
                               'conversionRate': 0,
                               'changeToBaseline': None,
                               'confidenceInterval': None,
                               'pValue': None,
                               'isBaseline': True if var_baseline == item else False,
                               'isWinner': False,
                               'isInvalid': True
                               })
        # Get winner variation
        listValid = [output.index(item) for item in output if not item['isInvalid']]
        # If at least one variation is valid:
        if len(listValid) != 0:
            dictValid = {}
            for index in listValid:
                dictValid[index] = output[index]['conversionRate']
            maxRateIndex = [k for k, v in sorted(dictValid.items(), key=lambda item: item[1])][-1]
            # when baseline has the highest conversion rate
            if output[maxRateIndex]['changeToBaseline'] > 0:
                output[maxRateIndex]['isWinner'] = True
    logger.info('ExptResults:')
    logger.info(output)
    # result to send to rabbitmq
    output_to_mq = {
        'ExperimentId': expt['ExptId'],
        'EventType': expt.get('EventType', None),
        'CustomEventTrackOption': expt.get('CustomEventTrackOption', None),
        'CustomEventUnit' : expt.get('CustomEventUnit', None),
        'CustomEventSuccessCriteria': expt.get('CustomEventSuccessCriteria', None),
        'IterationId': expt['IterationId'],
        'StartTime': expt['StartExptTime'],
        'EndTime': datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f"),
        'Results': output
    }
    return output_to_mq


def calc_customevent_numeric(expt, list_ff_events, list_user_events, logger=logging.getLogger(__name__)):
    # Set power and alpha default value for expt.
    para_power = 0.8
    para_alpha = 0.05
    ratio_power = 1.0
    # User's flags event aggregation, if not empty
    if list_ff_events:
        df_ff_events = pd.DataFrame(list_ff_events)
        ff_events_agg = df_ff_events.sort_values('TimeStamp').groupby('UserKeyId').last().reset_index().to_dict('records')
    else:
        ff_events_agg = []
    # User's custom event aggregation, if not empty
    if list_user_events:
        df_user_events = pd.DataFrame(list_user_events)
        user_events_agg = df_user_events.sort_values('TimeStamp').groupby('UserKeyId').mean().reset_index().to_dict('records')
    else:
        user_events_agg = []
    # Stat of Flag
    var_baseline = expt['BaselineVariation']
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
    logger.info('dictionary of flag var:occurence')
    logger.info(dict_var_occurence)

    for item in dict_var_user.keys():
        dict_var_user[item] = list(set(dict_var_user[item]))

    dict_expt_occurence = {}
    dict_var_listValues = {}
    for item in user_events_agg:
        user = item['UserKeyId']
        for it in dict_var_user.keys():
            if user in dict_var_user[it]:
                if it not in list(dict_expt_occurence.keys()):
                    dict_expt_occurence[it] = 1
                else:
                    dict_expt_occurence[it] = 1 + dict_expt_occurence[it]

    for item in list_user_events:
        user = item['UserKeyId']
        metricValue = item['NumericValue']
        for it in dict_var_user.keys():
            if user in dict_var_user[it]:
                if it not in list(dict_var_listValues.keys()):
                    dict_var_listValues[it] = [metricValue]
                else:
                    dict_var_listValues[it] = dict_var_listValues[it] + [metricValue]

    logger.info('dictionary of expt var:occurence')
    logger.info(dict_expt_occurence)

    # Get User Power & ExperimentEffect Expections
    # If not given, Use Power=0.8 as default value
    Compare_Power = False
    if not expt.get('Power', None):
        # Compare power value with 0.8 to decide isInvalid
        Power = para_power
        Compare_Power = True
    else:
        # Compare sample with minimum sample calculated
        Power = expt['Power']
        ExpectedExperimentEffect = expt['ExpectedExperimentEffect']

    output = []
    for var in expt['Variations']:
        if var not in dict_var_occurence.keys():
            output.append({'variation': var,
                           'totalEvents': None,
                           'average': None,
                           'changeToBaseline': None,
                           'confidenceInterval': None,
                           'pValue': None,
                           'isBaseline': True if var_baseline == var else False,
                           'isWinner': False,
                           'isInvalid': True
                           })
    # If  (baseline variation usage = 0) or (baseline variation customer event = 0 )
    if var_baseline not in list(dict_var_occurence.keys()) or var_baseline not in dict_expt_occurence.keys():
        # output only the flag-variation used.
        for item in dict_var_occurence.keys():
            if item in dict_expt_occurence.keys():
                dist_item = dict_var_listValues[item]
                rate, min, max = mean_confidence_interval(dist_item)
                if math.isnan(min) or math.isnan(max):
                    confidenceInterval = None
                else:
                    confidenceInterval = [0 if round(min, 3) < 0 else round(min, 3), round(max, 3)]
                pValue = -1
                output.append({'variation': item,
                               'totalEvents':len(dict_var_listValues[item]),
                               'average': round(rate, 3),
                               'changeToBaseline':  None,
                               'confidenceInterval': confidenceInterval,
                               'pValue': None,
                               'isBaseline': True if var_baseline == item else False,
                               'isWinner': False,
                               'isInvalid': True
                               })
            else:
                output.append({'variation': item,
                               'totalEvents': 0,
                               'average': None,
                               'changeToBaseline': None,
                               'confidenceInterval': None,
                               'pValue': None,
                               'isBaseline': True if var_baseline == item else False,
                               'isWinner': False,
                               'isInvalid': True
                               })
    else:
        BaselineRate = sum(dict_var_listValues[var_baseline]) / len(dict_var_listValues[var_baseline])
        # Preprare Baseline data sample distribution for Pvalue Calculation
        dist_baseline = dict_var_listValues[var_baseline]
        for item in dict_var_occurence.keys():
            if item in dict_expt_occurence.keys():
                dist_item = dict_var_listValues[item]
                rate, min, max = mean_confidence_interval(dist_item)
                if math.isnan(min) or math.isnan(max):
                    confidenceInterval = None,
                else:
                    confidenceInterval =  [0 if round(min, 3) < 0 else round(min, 3), round(max, 3)]
                pValue = round(stats.ttest_ind(dist_baseline, dist_item).pvalue, 2)
                # Calculate Power
                try:
                    s1 = int(dict_var_occurence[var_baseline])
                    s2 = int(dict_var_occurence[item])
                    sampleNow = s1 if s1 < s2 else s2
                    if Compare_Power:
                        # calculate power compared to default power value
                        Effect = (rate - BaselineRate) / BaselineRate
                        power_now = TTestIndPower.power(Effect, sampleNow, para_alpha)
                        if power_now > para_power:
                            power_valid = True
                        else:
                            power_valid = False
                        logger.info(f'calculate power compared to default value: {power_now}')
                    else:
                        # calculate miminum sample
                        required_n = ceil(tt_ind_solve_power(
                            ExpectedExperimentEffect,
                            power=Power,
                            alpha=para_alpha,
                            ratio=ratio_power)
                        )
                        if sampleNow > required_n:
                            power_valid = True
                        else:
                            power_valid = False
                        logger.info('calculate minimum sample size: {required_n}')
                except:
                    logger.info('ERROR in power calculation, return power_valid False')
                    power_valid = False

                if (pValue.value > para_alpha) or math.isnan(pValue.value):
                    pValue_valid = False
                else:
                    pValue_valid = True
                if pValue_valid and power_valid :
                    isInvalid = False
                else:
                    isInvalid = True
                    
                if math.isnan(pValue.value):
                    pValue = None

                output.append({'variation': item,
                               'totalEvents': len(dict_var_listValues[item]),
                               'average': round(rate, 3),
                               'changeToBaseline': round(rate, 3) - round(BaselineRate, 3),
                               'confidenceInterval': confidenceInterval,
                               'pValue': pValue,
                               'isBaseline': True if var_baseline == item else False,
                               'isWinner': False,
                               'isInvalid': isInvalid
                               })
            else:
                output.append({'variation': item,
                               'totalEvents': 0,
                               'average': None,
                               'changeToBaseline': None,
                               'confidenceInterval': None,
                               'pValue': None,
                               'isBaseline': True if var_baseline == item else False,
                               'isWinner': False,
                               'isInvalid': True
                               })
        # Get winner variation
        listValid = [output.index(item) for item in output if not item['isInvalid']]
        # If at least one variation is valid:
        if len(listValid) != 0:
            dictValid = {}
            for index in listValid:
                dictValid[index] = output[index]['average']
            if expt['CustomEventSuccessCriteria'] == 1 :
                # Max value wins
                maxRateIndex = [k for k, v in sorted(dictValid.items(), key=lambda item: item[1])][-1]
                # when baseline has the highest conversion rate
                if output[maxRateIndex]['changeToBaseline'] > 0:
                    output[maxRateIndex]['isWinner'] = True
            elif expt['CustomEventSuccessCriteria'] == 2 :
                # Min value wins
                minRateIndex = [k for k, v in sorted(dictValid.items(), key=lambda item: item[1])][0]
                # when baseline has the highest conversion rate
                if output[minRateIndex]['changeToBaseline'] < 0:
                    output[minRateIndex]['isWinner'] = True
            else:
                logger.info('ERROR : Non-Recognised CustomEventSuccessCriteria')
    logger.info('ExptResults:')
    logger.info(output)
    # result to send to rabbitmq
    output_to_mq = {
        'ExperimentId': expt['ExptId'],
        'EventType': expt['EventType'],
        'CustomEventTrackOption': expt['CustomEventTrackOption'],
        'CustomEventUnit' : expt['CustomEventUnit'],
        'CustomEventSuccessCriteria': expt['CustomEventSuccessCriteria'],
        'IterationId': expt['IterationId'],
        'StartTime': expt['StartExptTime'],
        'EndTime': datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f"),
        'Results': output
    }
    return output_to_mq
