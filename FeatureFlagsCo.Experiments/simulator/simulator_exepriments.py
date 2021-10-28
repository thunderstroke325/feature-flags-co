from random import choice

import requests

environmentSecret = 'ZTU0LWYzYTItNCUyMDIxMTAyODA3Mjk1M19fMl9fMl9fM19fZGVmYXVsdF9jZjY0ZQ=='
url_ffEvent = 'http://localhost:5000/Variation/GetMultiOptionVariation'
url_customEvent = 'http://localhost:5000/ExperimentsDataReceiver/PushData'

FF_NAME = 'testazuresb'
EVENT_NAME = 'clickme'

# ffEvent
for group in range(1, 4):
    for user in range(1, 1000):
        ffUserName = "u_group" + str(group) + "_" + str(user)
        data = {
            "featureFlagKeyName": FF_NAME,
            "environmentSecret": environmentSecret,
            "ffUserName": ffUserName,
            "ffUserEmail": ffUserName + "@testliang.com",
            "ffUserCountry": "China",
            "ffUserKeyId": ffUserName + "@testliang.com",
        }
        params = {'sessionKey': environmentSecret,
                  'format': 'xml', 'platformId': 1}
        print('ffUser: ' + ffUserName)
        response = requests.post(url_ffEvent, params=params, json=data)
        print(response.text)
        print('==================')

# User CustomEvent
for group in range(1, 4):
    weight = choice([i for i in range(100, 334)])
    for user in range(1, 1000 - weight * group):
        ffUserName = "u_group" + str(group) + "_" + str(user)
        data = [
            {
                "route": "index/paypage",
                "secret": environmentSecret,
                # "timeStamp": datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f"),
                "type": "CustomEvent",
                "eventName": EVENT_NAME,
                "user": {
                        "ffUserName": ffUserName,
                        "ffUserEmail": ffUserName + "@testliang.com",
                        "ffUserCountry": "China",
                        "ffUserKeyId": ffUserName + "@testliang.com",
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
        print('ffUser: ' + ffUserName)
        response = requests.post(url_customEvent, params=params, json=data)
        print(response.status_code)
        print('==================')
