###############################
# Dataset received from Q4 & Q5
###############################
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

###################################################
# Receive message from Rabbitmq
###################################################

#  ACTION : Get > Event


###################################################
# Python Code to store message to Redis 
###################################################

if event["_index"] =  "ffvariationrequestindex" : 
    
    # ACTION: Get > list_flags 
    list_acitve_flagsevents = {}
    if event["FeatureFlagId"] in list_actvie_flags:
        # ACTION: Get > flagevents[''] 
else:


