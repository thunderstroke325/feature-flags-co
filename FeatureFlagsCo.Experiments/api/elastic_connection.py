from elasticsearch import Elasticsearch
from config.config_handling import get_config_value


def connect_elasticsearch(**kwargs):
    _es_config = get_config_value('elastic', 'es_host')
    _es_hosts = [_es_config]
    _es_username= get_config_value('elasticusername', 'es_username')
    _es_passwd = get_config_value('elasticpasswd', 'es_passwd')
    if 'hosts' in kwargs.keys():
        _es_hosts = kwargs['hosts']
    _es_obj = None
    _es_obj = Elasticsearch(hosts=_es_hosts, http_auth=(_es_username, _es_passwd),timeout=30,max_retries=10,retry_on_timeout=True)

    if _es_obj.ping():
        print('Elasticsearch Connected!')
    else:
        print('Elasticsearch not connected!')
    return _es_obj