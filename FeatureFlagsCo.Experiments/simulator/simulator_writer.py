

from datetime import datetime
from config.config_handling import get_config_value
import logging
from time import sleep, time
from rabbitmq.rabbitmq import RabbitMQSender

logger = logging.getLogger("simulator_writer")
logger.setLevel(logging.INFO)

if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')

    mq_host = get_config_value('rabbitmq', 'mq_host')
    mq_port = get_config_value('rabbitmq', 'mq_port')
    mq_username = get_config_value('rabbitmq', 'mq_username')
    mq_passwd = get_config_value('rabbitmq', 'mq_passwd')
    redis_host = get_config_value('redis', 'redis_host')
    redis_port = get_config_value('redis', 'redis_port')
    redis_passwd = get_config_value('redis', 'redis_passwd')

    # Q1 start
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
    RabbitMQSender(mq_host,
                   mq_port,
                   mq_username,
                   mq_passwd,
                   redis_host,
                   redis_port,
                   redis_passwd).send('Q1', 'py.experiments.recordinginfo', Q1_START)
    sleep(30)

    for group in range(1, 4):
        for user in range(25):
            # Q4
            Q4 = {
                "RequestPath": "index/paypage",
                "FeatureFlagId": "FF__38__48__103__PayButton",
                "EnvId": "103",
                "AccountId": "38",
                "ProjectId": "48",
                "FeatureFlagKeyName": "PayButton",
                "UserKeyId": "u_group"+str(group)+"_"+str(user)+"@testliang.com",
                "FFUserName": "u_group"+str(group)+"_"+str(user),
                "VariationLocalId": str(group),
                "VariationValue": "Small-Button",
                "TimeStamp": datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f"),
                "phoneNumber": "135987652543"
            }
            Q5 = {
                "Route": "index",
                "Secret": "YjA1LTNiZDUtNCUyMDIxMDkwNDIyMTMxNV9fMzhfXzQ4X18xMDNfX2RlZmF1bHRfNzc1Yjg=",
                "TimeStamp":  datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f"),
                "Type": "CustomEvent",
                "EventName": "ButtonPayTrack",
                "User": {
                        "FFUserName": "u_group"+str(group)+"_"+str(user),
                        "FFUserEmail": "u_group"+str(group)+"_"+str(user)+"@testliang.com",
                        "FFUserCountry": "China",
                        "FFUserKeyId": "u_group"+str(group)+"_"+str(user)+"@testliang.com",
                        "FFUserCustomizedProperties": [
                            {
                                "Name": "age",
                                "Value": "16"
                            }
                        ]
                },
                "ApplicationType": "Javascript",
                "CustomizedProperties": [
                    {
                        "Name": "age",
                        "Value": "16"
                    }
                ],
                "ProjectId": "48",
                "EnvironmentId": "103",
                "AccountId": "38"
            }
            RabbitMQSender(mq_host,
                           mq_port,
                           mq_username,
                           mq_passwd,
                           redis_host,
                           redis_port,
                           redis_passwd).send('Q4', 'py.experiments.events.ff', Q4)

            RabbitMQSender(mq_host,
                           mq_port,
                           mq_username,
                           mq_passwd,
                           redis_host,
                           redis_port,
                           redis_passwd).send('Q5', 'py.experiments.events.user', Q5)

    sleep(30)
    #  Q1 end
    Q1_END = {
        "ExptId": 'FF__38__48__103__PayButton_exp1',
        "IterationId": "2",
        "EnvId": "103",
        "FlagId": "FF__38__48__103__PayButton",
        "BaselineVariation": "1",
        "Variations": ["1", "2", "3"],
        "EventName": "ButtonPayTrack",
        "StartExptTime": "2021-09-20T21:00:00.123456",
        "EndExptTime": "2021-10-01T21:00:00.123456"
    }
    RabbitMQSender(mq_host,
                   mq_port,
                   mq_username,
                   mq_passwd,
                   redis_host,
                   redis_port,
                   redis_passwd).send('Q1', 'py.experiments.recordinginfo', Q1_END)
    logger.info('send to Q1 expt end')
