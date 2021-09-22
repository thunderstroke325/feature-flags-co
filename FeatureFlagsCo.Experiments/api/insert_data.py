from flask.wrappers import Response
from ExperimentApiPy import app
from api.elastic_connection import connect_elasticsearch
from flask import jsonify, request
from datetime import datetime
from elasticsearch import helpers
import os, uuid

###########################################
# Insert test flag usage events in bulk to 
# Elastic index : "ffvariationrequestindex"
###########################################

es = connect_elasticsearch()

@app.route('/api/InsertFlagUsageEvent', methods=['GET'])
def add_flagevent():
    Nevents=1000
    actions = []
    dict_value = {1:"Big-Button", 3:"Normal-Button", 2:"Small-Button"}
    start = datetime.now()
    for group in range(3):
      for doc in range(Nevents):
        data={
                "_index": "ffvariationrequestindex",
                "_type": "_doc",
                "_id" : uuid.uuid4(), # random UUID for _id
                "doc_type" : "_doc", # document _type
                "_source": { # the body of the document
                      "RequestPath" : "index/paypage",
                      "FeatureFlagId" : "FF__38__48__103__PayButton",
                      "EnvId" : "103",
                      "AccountId" : "38",
                      "ProjectId" : "48",
                      "FeatureFlagKeyName" : "PayButton",
                      "UserKeyId" : "u_group"+str(group)+"_"+str(doc)+"@testliang.com",
                      "FFUserName" : "u_group"+str(group)+"_"+str(doc),
                      "VariationLocalId" : str(group+1),
                      "VariationValue" : dict_value[group+1],
                      "TimeStamp" : datetime.now().strftime("%Y-%m-%dT%H:%M:%S"),
                      "phoneNumber" : "135987652543"
                }
              }
        actions.append(data) 
    try:
      # make the bulk call, and get a response
      response = helpers.bulk(es, actions)
      print ("\nRESPONSE:", response)
    except Exception as e:
      print("\nERROR:", e)
    delta = datetime.now() - start
    return jsonify({
      "status":"succesfully sent "+str(Nevents*3)+' events to index ffvariationrequestindex', 
      "delta_time_s": delta.total_seconds()
    })

'''
# Push one by one
    for group in range(3):
      for user in range(10000):
        N = "u_group"+str(group)+"_"+str(user)
        user_obj = {
          "RequestPath" : "index/pay",
          "FeatureFlagId" : "FF__38__48__103__test-liang",
          "EnvId" : "103",
          "AccountId" : "38",
          "ProjectId" : "48",
          "FeatureFlagKeyName" : "test-liang",
          "UserKeyId" : N+"@testliang.com",
          "FFUserName" : N,
          "VariationLocalId" : "1",
          "VariationValue" : dict_value[group],
          "TimeStamp" : datetime.now(),
          "phoneNumber" : "135987652543"
        }
        print(user)
        result = es.index(index='ffvariationrequestindex',  body=user_obj, request_timeout=10)
'''


###########################################
# Insert test custom events to 
# Elastic index : "experiments"
###########################################
@app.route('/api/InsertCustomEvent', methods=['GET'])
def add_customevent():
    Nevents=1000
    actions = []
    start = datetime.now()
    for group in range(3):
      for doc in range(Nevents - (group+1)*200):
        data={
                "_index": "experiments",
                "_type": "_doc",
                "_id" : uuid.uuid4(), # random UUID for _id
                "doc_type" : "_doc", # document _type
                "_source": { # the body of the document
                        "Route" : "index",
                        "Secret" : "Yoursecret",
                        "Type" : "CustomEvent",
                        "TimeStamp" : datetime.now().strftime("%Y-%m-%dT%H:%M:%S"),
                        "EventName" : "clickButtonPayTrack",
                        "User" : {
                          "FFUserName" : "u_group"+str(group)+"_"+str(doc),
                          "FFUserEmail" : "u_group"+str(group)+"_"+str(doc)+"@testliang.com",
                          "FFUserCountry" : "China",
                          "FFUserKeyId" : "u_group"+str(group)+"_"+str(doc)+"@testliang.com",
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
        actions.append(data) 
    try:
          # make the bulk call, and get a response
          response = helpers.bulk(es, actions, index='experimentss', doc_type='_doc')
          print ("\nRESPONSE:", response)
    except Exception as e:
          print("\nERROR:", e)
    delta = datetime.now() - start
    return jsonify({
      "status":"succesfully sent "+str(Nevents*3)+' events to index experiments',
      "delta_time_s": delta.total_seconds()
    })

'''
# Push one by one
    group = 0
    for user in range(1):
        N = "u_group"+str(group)+"_"+str(user)
        user_obj = {
          "Route" : "index",
          "Secret" : "YourSecret",
          "TimeStamp" : datetime.now().strftime('%s')+'000',
          "Type" : "CustomEvent",
          "EventName" : "clickButtonPaytest",
          "User" : {
            "FFUserName" : N,
            "FFUserEmail" : N+"@testliang.com",
            "FFUserCountry" : "China",
            "FFUserKeyId" : N+"@testliang.com",
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
        result = es.index(index='experiments',  body=user_obj, request_timeout=10)
        
    return jsonify(result)
'''