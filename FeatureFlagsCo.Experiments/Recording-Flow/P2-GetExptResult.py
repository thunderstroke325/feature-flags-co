import numpy as np
import scipy as sp
from scipy import stats
from datetime import datetime
import math
import pandas as pd

##############################################
# 0. Receive Data from Q2
# 1. Get event data from Redis
# 2. calculate Results, send to Q3
# 3. if Expt stoped, delete Redis data
#    else , send back exptID to Q2
###############################################

startime = datetime.now()
##########################
#0. Receive message from Q2
##########################

# ACTION: Get from Q2 > ExptID:
ExptID = 'FF_38_48_103_PayButton_expt1'

##########################
#1. Get event data from Redis
##########################

# ACTION : Get > ExptInfo
ExptInfo = {
            "EventName": "clickButtonPayTrack",
            "StartExptTime": "2021-09-21T16:15:00",
            "EndExptTime": "",
            "Flag": {
                "Id": "FF_38_48_103_PayButton",
                "BaselineVariation": "3",
                "Variations": [ "1","2","3"
                ]
            }
}
FlagID = ExptInfo["Flag"]["Id"]
EventName = ExptInfo["EventName"]

# ACTION : Send to Redis Flag ID, Get from Redis List of FF data >
list_FFevents  =   [
                        {
                            "FeatureFlagId" : "FF__38__48__103__PayButton",
                            "UserKeyId" : "u_group1_0@testliang.com",
                            "VariationLocalId" : "2",
                            "TimeStamp" : "2021-09-20T23:42:21"
                        },
                        {
                            "FeatureFlagId" : "FF__38__48__103__PayButton",
                            "UserKeyId" : "u_group1_1@testliang.com",
                            "VariationLocalId" : "2",
                            "TimeStamp" : "2021-09-20T23:42:21"
                        }
]

# ACTION : Send to Redis Flag ID, Get from Redis List of FF data >
list_Exptevents =    [
                        {
                            "EventName" : "clickButtonPayTrack",
                            "UserKeyId" : "u_group0_0@testliang.com",
                            "TimeStamp" : "2021-09-20T23:42:43"
                        },
                        {
                            "EventName" : "clickButtonPayTrack",
                            "UserKeyId" : "u_group0_1@testliang.com",
                            "TimeStamp" : "2021-09-20T23:42:43"
                        }
]        

##########################
# 2. Calculate ExptResult
##########################
# FFevent User Aggregation 
df_FF = pd.DataFrame(list_FFevents)
list_FF_agg = df_FF.sort_values("TimeStamp").groupby("UserKeyId").last().reset_index().to_dict(orient='record')

# Exptevent User Aggregation 
df_Expt = pd.DataFrame(list_Exptevents)
list_Expt_agg = df_Expt.sort_values("TimeStamp").groupby("UserKeyId").last().reset_index().to_dict(orient='record')

# Stat of Flag
var_baseline = ExptInfo['Flag']['BaselineVariation']
dict_var_user = {}
dict_var_occurence = {}

for item in list_FF_agg:
    value = item['VariationLocalId']
    user  = item['UserKeyId']
    if  value not in list(dict_var_occurence.keys()) :
        dict_var_occurence[ value ]  = 1
        dict_var_user[ value ]  = [user]
    else :
        dict_var_occurence[ value] = dict_var_occurence[ value] + 1
        dict_var_user[ value ]  =  dict_var_user[ value] + [user]

print('dictionary of flag var:occurence')
print(dict_var_occurence)  
    
for item in dict_var_user.keys():
    dict_var_user[item] = list(set(dict_var_user[item]))


# Stat of Expt.
dict_expt_occurence = {}
for item in list_FFevents:
    user  = item['UserKeyId']
    for it in dict_var_user.keys():
        if user in dict_var_user[it] :
            if it not in list(dict_expt_occurence.keys()):
                dict_expt_occurence[it] = 1
            else:
                dict_expt_occurence[it] = 1 + dict_expt_occurence[it]

print('dictionary of expt var:occurence')
print(dict_expt_occurence)

# Get Confidence interval
def mean_confidence_interval(data, confidence=0.95):
    a = 1.0 * np.array(data)
    n = len(a)
    m, se = np.mean(a), sp.stats.sem(a)
    h = se * sp.stats.t.ppf((1 + confidence) / 2., n-1)
    return m, m-h, m+h

output = []
# If  (baseline variation usage = 0) or (baseline variation customer event = 0 )
if var_baseline not in list(dict_var_occurence.keys()) or var_baseline not in dict_expt_occurence.keys():
    for item in dict_var_occurence.keys(): 
        if item in  dict_expt_occurence.keys():
            dist_item = [1 for i in range(dict_expt_occurence[item])] + [0 for i in range(dict_var_occurence[item]-dict_expt_occurence[item])]
            rate, min, max = mean_confidence_interval(dist_item)
            if math.isnan(min) or math.isnan(max):
                confidenceInterval = [-1, -1]
            else:
                confidenceInterval = [ 0 if round(min,2)<0 else round(min,2), 1 if round(max,2)>1 else round(max,2)]
            pValue = -1
            output.append({ 'variation': item,
                            'conversion' : dict_expt_occurence[item],  
                            'uniqueUsers' : dict_var_occurence[item], 
                            'conversionRate':   round(rate,2),
                            'changeToBaseline' : -1,
                            'confidenceInterval': confidenceInterval, 
                            'pValue': -1 ,
                            'isBaseline': True if ExptInfo['Flag']['BaselineVariation'] == item else False, 
                            'isWinner': False,
                            'isInvalid': True 
                            } 
                        )
else:
    BaselineRate = dict_expt_occurence[var_baseline]/dict_var_occurence[var_baseline]
    # Preprare Baseline data sample distribution for Pvalue Calculation 
    dist_baseline = [1 for i in range(dict_expt_occurence[var_baseline])] + [0 for i in range(dict_var_occurence[var_baseline]-dict_expt_occurence[var_baseline])] 
    for item in dict_var_occurence.keys(): 
        if item in  dict_expt_occurence.keys():
            dist_item = [1 for i in range(dict_expt_occurence[item])] + [0 for i in range(dict_var_occurence[item]-dict_expt_occurence[item])]
            rate, min, max = mean_confidence_interval(dist_item)
            if math.isnan(min) or math.isnan(max):
                confidenceInterval = [-1, -1]
            else:
                confidenceInterval = [ 0 if round(min,2)<0 else round(min,2), 1 if round(max,2)>1 else round(max,2)]
            pValue = round(1-stats.ttest_ind(dist_baseline, dist_item).pvalue,2)
            output.append({ 'variation': item,
                            'conversion' : dict_expt_occurence[item],  
                            'uniqueUsers' : dict_var_occurence[item], 
                            'conversionRate':   round(rate,3),
                            'changeToBaseline' : round(rate,3) - round(BaselineRate,3),
                            'confidenceInterval': confidenceInterval, 
                            'pValue': -1 if math.isnan(pValue) else pValue,
                            'isBaseline': True if ExptInfo['Flag']['BaselineVariation'] == item else False, 
                            'isWinner': False,
                            'isInvalid': True if (pValue < 0.95) or math.isnan(pValue) else False
                            } 
                        )
    # Get winner variation
    listValid = [output.index(item) for item in output if item['isInvalid'] == False]

    # If at least one variation is valid:
    if len(listValid) != 0: 
        dictValid = {}
        for index in listValid:
            dictValid[index] = output[index]['conversionRate']
        maxRateIndex = [k for k, v in sorted(dictValid.items(), key=lambda item: item[1])][-1]    
        # when baseline has the highest conversion rate
        if output[maxRateIndex]['changeToBaseline'] > 0:  
            output[maxRateIndex]['isWinner'] = True

print('ExptResults:')
print(output)

output_to_mq = {
                    "ExperimentId" : ExptID, 
                    "StartTime": ExptInfo["StartExptTime"],
                    "EndTime": startime,
                    "Results": output
                } 
# ACTION : Send to Rabbitmq Q3 > output_to_mq

##########################
# 3. if Expt stoped, delete Redis data
#    else: send ExptID back to Q2
##########################
if ExptInfo["EndExptTime"] :
    # Update dict_flag_acitveExpts
    # ACTION : Get from Redis > dict_flag_acitveExpts
    dict_flag_acitveExpts = {'FF_38_48_103_PayButton': 
                                ['FF_38_48_103_PayButton_expt1', 
                                'FF_38_48_103_PayButton_expt2'
                                ]
                            }   
    dict_flag_acitveExpts[FlagID].remove(ExptID)
    # ACTION: Send to Redis > dict_flag_acitveExpts

    # Update dict_flag_acitveExpts
    # ACTION : Get from Redis > dict_flag_acitveExpts
    dict_customEvent_acitveExpts = {'clickButtonPayTrack': 
                                ['FF_38_48_103_PayButton_expt1', 
                                'FF_38_48_103_PayButtonColor_expt1'
                                ]
                            } 
    dict_customEvent_acitveExpts[EventName].remove(ExptID)
    # ACTION: Send to Redis > dict_customEvent_acitveExpts
    # ACTION: Delete in Redis > list_FFevent related to FlagID
    # ACTION: Delete in Redis > list_Exptevent related to EventName

    print('Update info and delete stopped Experiment data')
else:
    # Send ExptID back to Q2
    print('send back to Q2 ExptID')

endtime = datetime.now()
print('processing time:') 
print((endtime-startime))




