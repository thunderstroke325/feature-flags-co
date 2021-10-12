import requests
from datetime import datetime

environmentSecret = 'ZDU0LWNlOTItNCUyMDIxMTAxMDIxMzY1MF9fMl9fMl9fM19fZGVmYXVsdF8zM2MwNA=='
url_ffEvent = 'http://localhost:5000/Variation/GetMultiOptionVariation'
url_customEvent = 'http://localhost:5000/ExperimentsDataReceiver/PushData'

# ffEvent
for group in range(0, 3):
    for user in range(20, 30):
        ffUserName = "u_group"+str(group)+"_"+str(user)
        data = {
            "featureFlagKeyName": "hahaha",
            "environmentSecret": environmentSecret,
            "ffUserName": ffUserName,
            "ffUserEmail": ffUserName+"@testliang.com",
            "ffUserCountry": "China",
            "ffUserKeyId": ffUserName+"@testliang.com",
        }
        params = {'sessionKey': environmentSecret,
                  'format': 'xml', 'platformId': 1}
        print('ffUser: '+ffUserName)
        response = requests.post(url_ffEvent, params=params, json=data)
        print(response.text)
        print('==================')

# User CustomEvent
for group in range(0, 3):
    for user in range(0, 20-4*group):
        ffUserName = "u_group"+str(group)+"_"+str(user)
        data = [
            {
                "route": "index/paypage",
                "secret": environmentSecret,
                # "timeStamp": datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f"),
                "type": "CustomEvent",
                "eventName": "clickme",
                "user": {
                        "ffUserName": ffUserName,
                        "ffUserEmail": ffUserName+"@testliang.com",
                        "ffUserCountry": "China",
                        "ffUserKeyId": ffUserName+"@testliang.com",
                        "ffUserCustomizedProperties": [
                            {
                                "name": "string",
                                "value": "string"
                            }
                        ]
                },
                "appType": "PythonApp",
                "customizedProperties": [
                    {
                        "name": "string",
                        "value": "string"
                    }
                ]
            }
        ]
        params = {'sessionKey': environmentSecret,
                  'format': 'xml', 'platformId': 1}
        print('ffUser: '+ffUserName)
        response = requests.post(url_customEvent, params=params, json=data)
        print(response.status_code)
        print('==================')
