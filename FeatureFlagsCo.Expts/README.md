# ExperimentApiPy

Under "ExperimentApiPy/config", create file "config.ini".
In this file, put lines:

    [elastic]
    es_host =  http://localhost:9200

    [elasticusername]
    es_username =  elastic_username

    [elasticpasswd]
    es_passwd =  elastic_password
    
Then repalce localhost by your elasticsearch url. Replace elastic_username by your username, replace elastic_password by your password.


Run:

    pip install -r requirements.txt
    python ExperimentApiPy.py


To Insert Test Flag Usage Data to ElasticSearch:

    curl localhost:5000/api/InsertFlagUsageEvent

To Insert Test Custom Event Data to ElasticSearch, add at first your secret to api/insert_data.py : "Secret" : "YourSecret" :

    curl localhost:5000/api/InsertCustomEvent

To Get Data from ElasticSearch according Index and Id:

    curl localhost:5000/GetData -d '{"index":"experiments", "id":"d_pNsHsBFqb7-mBbFsuM"}' -H 'Content-Type: application/json'

To Search Data from ElasticSearch according Index and Type:


    curl localhost:5000/SearchDataByIndexAndKey -d '{"index":"experiments", "key":"Type", "value":"pageview"}' -H 'Content-Type: application/json'


To Get Experiement Result from ElasticSearch :

    curl localhost:5000/api/ExperimentResults -d '{"Flag" : {"Id": "FF__2__2__4__ffc-multi-variation-cache-test-data1-1630579986592","BaselineVariation": "A", "Variations" : ["A","Green"]}, "EventName": "TestEvent", "StartExptTime": "-2183958941000", "EndExptTime": "7283148259000"}' -H 'Content-Type: application/json'

"StartExptTime" : ""  When No Start Time selected, same for "EndExptTime".


Visualiz the Json data in : https://jsongrid.com/json-grid

