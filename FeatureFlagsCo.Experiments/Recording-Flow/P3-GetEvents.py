###############################
# Get events from Rabbitmq and 
# store userful info to Redis 
###############################

# Dataset received from Q4 & Q5
FFevent1 = {
        "_index" : "ffvariationrequestindex",
        "_type" : "_doc",
        "_id" : "377f89c7-95ae-4c99-9e79-3a960a544546",
        "_score" : 1.287705,
        "_source" : {
          "RequestPath" : "index/paypage",
          "FeatureFlagId" : "FF__38__48__103__PayButton",
          "EnvId" : "103",
          "AccountId" : "38",
          "ProjectId" : "48",
          "FeatureFlagKeyName" : "PayButton",
          "UserKeyId" : "u_group1_0@testliang.com",
          "FFUserName" : "u_group1_0",
          "VariationLocalId" : "2",
          "VariationValue" : "Small-Button",
          "TimeStamp" : "2021-09-20T23:42:21",
          "phoneNumber" : "135987652543"
        }
      }
FFevent2 = {
        "_index" : "ffvariationrequestindex",
        "_type" : "_doc",
        "_id" : "7d6d7486-416f-434a-bdc0-aaf6a412dd41",
        "_score" : 1.287705,
        "_source" : {
          "RequestPath" : "index/paypage",
          "FeatureFlagId" : "FF__38__48__103__PayButton",
          "EnvId" : "103",
          "AccountId" : "38",
          "ProjectId" : "48",
          "FeatureFlagKeyName" : "PayButton",
          "UserKeyId" : "u_group1_1@testliang.com",
          "FFUserName" : "u_group1_1",
          "VariationLocalId" : "2",
          "VariationValue" : "Small-Button",
          "TimeStamp" : "2021-09-20T23:42:21",
          "phoneNumber" : "135987652543"
        }
      }

Exptevent1 = {
        "_index" : "experiments",
        "_type" : "_doc",
        "_id" : "4cf8a6a0-28d7-4e7e-bb1d-5bcc8040ce95",
        "_score" : 1.0761821,     
        "_source" : {
          "Route" : "index",
          "Secret" : "YjA1LTNiZDUtNCUyMDIxMDkwNDIyMTMxNV9fMzhfXzQ4X18xMDNfX2RlZmF1bHRfNzc1Yjg=",
          "TimeStamp" : "2021-09-20T23:42:43",
          "Type" : "CustomEvent",
          "EventName" : "clickButtonPayTrack",
          "User" : {
            "FFUserName" : "u_group0_0",
            "FFUserEmail" : "u_group0_0@testliang.com",
            "FFUserCountry" : "China",
            "FFUserKeyId" : "u_group0_0@testliang.com",
            "FFUserCustomizedProperties" : [ 
                            {
              "Name" : "age",
              "Value" : "16"
            }
            ]
          },
          "AppType" : "Javascript",
          "CustomizedProperties" : [
            {
              "Name" : "age",
              "Value" : "16"
            }
          ],
          "ProjectId" : "48",
          "EnvironmentId" : "103",
          "AccountId" : "38"
        }
      }
Exptevent2 = {
        "_index" : "experiments",
        "_type" : "_doc",
        "_id" : "04dff239-b781-484c-bee9-15178b646bbf",
        "_score" : 1.0761821,
        "_source" : {
          "Route" : "index",
          "Secret" : "YjA1LTNiZDUtNCUyMDIxMDkwNDIyMTMxNV9fMzhfXzQ4X18xMDNfX2RlZmF1bHRfNzc1Yjg=",
          "TimeStamp" : "2021-09-20T23:42:43",
          "Type" : "CustomEvent",
          "EventName" : "clickButtonPayTrack",
          "User" : {
            "FFUserName" : "u_group0_1",
            "FFUserEmail" : "u_group0_1@testliang.com",
            "FFUserCountry" : "China",
            "FFUserKeyId" : "u_group0_1@testliang.com",
            "FFUserCustomizedProperties" : [ ]
          },
          "AppType" : "Javascript",
          "CustomizedProperties" : [
            {
              "Name" : "age",
              "Value" : "16"
            }
          ],
          "ProjectId" : "48",
          "EnvironmentId" : "103",
          "AccountId" : "38"
        }
      }

################################
# Receive message from Rabbitmq
################################
#  ACTION : Get from Q4/Q5 > Event
messageQueue = 'Q4'
if messageQueue == 'Q4':
    event = FFevent1
elif messageQueue == 'Q5':
    event = Exptevent1

########################################
# Python Code to store message to Redis 
########################################

if event["_index"] ==  "ffvariationrequestindex" : 
    # ACTION: Get from Redis > dict_flag_acitveExpts 
    dict_flag_acitveExpts = {'FF_38_48_103_PayButton': 
                                ['FF_38_48_103_PayButton_expt1', 
                                'FF_38_48_103_PayButton_expt2'
                                ]
                            }    
    if event["FeatureFlagId"] in dict_flag_acitveExpts.keys():
        # ACTION: Send FlagId to Redis, Get from Redis, can be empty list [] > list_FFevents 
        list_FFevents =  [
            {
                "FeatureFlagId" : FFevent1["_souce"]["FeatureFlagId"],
                "UserKeyId" : FFevent1["_souce"]["UserKeyId"],
                "VariationLocalId" : FFevent1["_souce"]["VariationLocalId"],
                "TimeStamp" : FFevent1["_souce"]["TimeStamp"]
            }
        ]
        dict_to_add = {
                "FeatureFlagId" : event["_souce"]["FeatureFlagId"],
                "UserKeyId" : event["_souce"]["UserKeyId"],
                "VariationLocalId" : event["_souce"]["VariationLocalId"],
                "TimeStamp" : event["_souce"]["TimeStamp"]
        }
        list_FFevents =  list_FFevents + [dict_to_add] 
        # ACTION : Send to Redis > list_FFevents related to an FlagID
elif event["_index"] ==  "experiments":
    # ACTION: Get from Redis > dict_customEvent_acitveExpts 
    dict_customEvent_acitveExpts = {'clickButtonPayTrack': 
                                ['FF_38_48_103_PayButton_expt1', 
                                'FF_38_48_103_PayButtonColor_expt1'
                                ]
                            }    
    if event["EventName"] in dict_customEvent_acitveExpts.keys():
        # ACTION: Send EventName to Redis, Get from Redis, can be empty list [] > list_Exptevents 
        list_Exptevents =  [
            {
                "EventName" : Exptevent1["_souce"]["EventName"],
                "UserKeyId" : Exptevent1["_souce"]["User"]["FFUserKeyId"],
                "TimeStamp" : Exptevent1["_souce"]["TimeStamp"]
            }
        ]        
        Exptevent2 = {
                "EventName" : event["_souce"]["EventName"],
                "UserKeyId" : event["_souce"]["User"]["FFUserKeyId"],
                "TimeStamp" : event["_souce"]["TimeStamp"]
        }
        list_Exptevents = list_Exptevents + [Exptevent2]
        # ACTION : Send to Redis > list_Exptevents related to an EventName