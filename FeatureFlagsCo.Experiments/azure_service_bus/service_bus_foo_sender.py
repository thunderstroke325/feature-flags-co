import logging
from azure_service_bus.send_consume import AzureSender
from experiment.utils import encode
from config.config_handling import get_config_value


TOPIC_NAME = 'ds'
ORIGIN = 'py'
Q1_START = {
    "ExptId": 'FF__38__48__103__PayButton_exp1',
    "IterationId": "2",
    "EnvId": "103",
    "FlagId": "FF__38__48__103__PayButton",
    "BaselineVariation": "1",
    "Variations": ["1", "2", "3"],
    "EventName": "ButtonPayTrack",
    "StartExptTime": "2021-09-20T21:00:00.123456",
    "EndExptTime": ""
}

if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    sb_host = get_config_value('azure', 'fully_qualified_namespace')
    sb_sas_policy = get_config_value('azure', 'sas_policy')
    sb_sas_key = get_config_value('azure', 'servicebus_sas_key')
    sender = AzureSender(sb_host, sb_sas_policy, sb_sas_key)
    with sender.redis.pipeline() as pipeline:
        values = [encode(Q1_START) for _ in range(5)]
        pipeline.rpush('FOO_LIST_KEY', *values)
        pipeline.sadd('FOO_SET_KEY', *values)
        pipeline.execute()
    sender.send(Q1_START, topic=TOPIC_NAME, subscription=ORIGIN)
