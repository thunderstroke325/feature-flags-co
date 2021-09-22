from flask.wrappers import Response
from ExperimentApiPy import app
from flask import jsonify, request
from datetime import datetime
import requests
from config.config_handling import get_config_value
from requests.auth import HTTPBasicAuth 
import json

_es_config = get_config_value('elastic', 'es_host')
_es_hosts = _es_config
_es_username= get_config_value('elasticusername', 'es_username')
_es_passwd = get_config_value('elasticpasswd', 'es_passwd')

@app.route('/api/nosdk', methods=['POST'])
def nosdk():

    #########################################################
    #inputdata json format:
    #  {'Flag' : 
    #        {'Id' : FlagId,
    #         'BaselineVariation': 'var2', 
    #          'Variations: [value1, value2, value3]
    #          }, 
    #    'EventName': xxx,
    #    'StartExptTime': '946731600000', 
    #    'EndExptTime': Timestamp2
    #   }
    ##########################################################
   
    # Get data from frontend
    Index = "ffvariationrequestindex"
    startime = datetime.now()
    data = request.get_json()
    # Query Flag data
    query_body_A = {
                "from": 0,
                "query": {
                    "bool": {
                    "must": [
                        {
                        "match": {
                            'FeatureFlagId.keyword': data['Flag']['Id'] 
                        }
                        }, {
                                "match": {
                                    "EnvId.keyword": data['Flag']['Id'].split('__')[3]
                                }
                        }
                    ],
                    "filter": {
                        "range": {
                            "TimeStamp": {
                                # when no start time selected: defaut time to 2000-01-01:01H
                                "gte": "2000-01-01T01:00:00" if data['StartExptTime'] == "" else data['StartExptTime'],
                                # when no end time selected: defaut time to NOW
                                "lte":  datetime.now() if  data['EndExptTime'] == "" else data['EndExptTime']
                            }
                        }
                    }
                    }
                },
                "aggs": {
                    "keys": {
                    "composite": {
                        "sources": [
                        {
                            "UserKeyId": {
                            "terms": {
                                "field": "UserKeyId.keyword"
                            }
                            }
                        },
                        {
                            "VariationLocalId": {
                            "terms": {
                                "field": "VariationLocalId.keyword"
                            }
                            }
                        }
                        ],
                        "size": 10000
                    }
                    }
                },
                "size": 0
                }
                    
    
    r1 = requests.post(_es_hosts + '/ffvariationrequestindex/_search', auth = HTTPBasicAuth(_es_username, _es_passwd), headers={"content-type":"application/json"} ,data = json.dumps(query_body_A))
    endtime1 = datetime.now()

    query_body_B ={
                    "from": 0,
                    "query": {
                        "bool": {
                            "must": [{
                                "match": {
                                            'EventName.keyword' : data['EventName']
                                }
                            }, {
                                "match": {
                                    "EnvironmentId.keyword": data['Flag']['Id'].split('__')[3]
                                }
                            }],
                            "filter": {
                                "range": {
                                    "TimeStamp": {
                                            # when no start time selected: defaut time to 2000-01-01:01H
                                            "gte": "2000-01-01T01:00:00" if data['StartExptTime'] == "" else data['StartExptTime'],
                                            # when no end time selected: defaut time to NOW
                                            "lte":  datetime.now() if  data['EndExptTime'] == "" else data['EndExptTime']
                                    }
                                }
                            }
                        }
                    },
                    "aggs": {
                        "keys": {
                            "composite": {
                                "sources": [{
                                    "UserKeyId": {
                                        "terms": {
                                            "field": "User.FFUserKeyId.keyword"
                                        }
                                    }
                                }],
                                "size": 10000
                            }
                        }
                    },
                    "size": 0
                }

    r2 = requests.post(_es_hosts + '/ffvariationrequestindex/_search', auth = HTTPBasicAuth(_es_username, _es_passwd), headers={"content-type":"application/json"} ,data = json.dumps(query_body_B))
    endtime2 = datetime.now()

    results = {
        "request1": {"timeInSeconds": (endtime1-startime).total_seconds(), "data": r1.json()},
        "request2": {"timeInSeconds": (endtime2-endtime1).total_seconds(), "data": r2.json()}
    }
    return jsonify(results)