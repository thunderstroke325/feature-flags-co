from random import choice

import requests

# environmentSecret = 'YjA1LTNiZDUtNCUyMDIxMDkwNDIyMTMxNV9fMzhfXzQ4X18xMDNfX2RlZmF1bHRfNzc1Yjg='
# url_ffEvent = 'https://ffc-api-ce2-dev.chinacloudsites.cn/Variation/GetMultiOptionVariation'
# url_customEvent = 'https://ffc-api-ce2-dev.chinacloudsites.cn/ExperimentsDataReceiver/PushData'
environmentSecret = 'ZDMzLTY3NDEtNCUyMDIxMTAxNzIxNTYyNV9fMzZfXzQ2X185OF9fZGVmYXVsdF80ODEwNA=='
url_ffEvent = 'https://api.feature-flags.co/Variation/GetMultiOptionVariation'
url_customEvent = 'https://api.feature-flags.co/ExperimentsDataReceiver/PushData'


FF_NAME = ('PayButton', True)
EVENT_NAMES = [('click_c9a9fc3e-5b78-4116-809d-52a71bfadb01', False),
               ('pageview_63f2d5d1-038c-448e-94f6-84d55ac17d7b', False),
               ('ButtonPayTrack', True)]

# ffEvent
ff_name, is_run = FF_NAME
if is_run:
    for group in range(0, 5):
        for user in range(1, 100):
            ffUserName = "u_group" + str(group) + "_" + str(user)
            data = {
                "featureFlagKeyName": ff_name,
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

for event_name, is_run in EVENT_NAMES:
    # User CustomEvent
    if is_run:
        for group in range(0, 5):
            weight = choice([i for i in range(10, 20)])
            for user in range(1, 100 - weight * (group + 1)):
                ffUserName = "u_group" + str(group) + "_" + str(user)
                data = [
                    {
                        "route": "index/paypage",
                        "secret": environmentSecret,
                        # "timeStamp": datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f"),
                        "type": "CustomEvent",
                        "eventName": event_name,
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
                params = {'sessionKey': environmentSecret, 'format': 'xml', 'platformId': 1}
                print('ffUser: ' + ffUserName)
                response = requests.post(url_customEvent, params=params, json=data)
                print(response.status_code)
                print('==================')
