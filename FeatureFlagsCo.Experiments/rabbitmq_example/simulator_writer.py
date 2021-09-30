

from config.config_handling import get_config_value
import logging
from time import sleep, time
from rabbitmq.rabbitmq import RabbitMQSender

__CONST_Q1_START = {
    "ExptId": 'FF__38__48__103__PayButton_exp1',
    "IterationId": "1",
    "EnvId": "103",
    "FlagId": "FF__38__48__103__PayButton",
    "BaselineVariation": "1",
    "Variations": ["1", "2", "3"],
    "EventName": "clickButtonPayTrack",
    "StartExptTime": "2021-09-20T21:00:00.123456",
    "EndExptTime": ""
}

__CONST_Q1_END = {
    "ExptId": 'FF__38__48__103__PayButton_exp1',
    "IterationId": "1",
    "EnvId": "103",
    "FlagId": "FF__38__48__103__PayButton",
    "BaselineVariation": "1",
    "Variations": ["1", "2", "3"],
    "EventName": "clickButtonPayTrack",
    "StartExptTime": "2021-09-20T21:00:00.123456",
    "EndExptTime": "2021-09-20T23:50:00.123456"
}

__CONST_Q4 = {
    "RequestPath": "index/paypage",
    "FeatureFlagId": "FF__38__48__103__PayButton",
    "EnvId": "103",
    "AccountId": "38",
    "ProjectId": "48",
    "FeatureFlagKeyName": "PayButton",
    "UserKeyId": "u_group1_0@testliang.com",
    "FFUserName": "u_group1_0",
    "VariationLocalId": "2",
    "VariationValue": "Small-Button",
    "TimeStamp": "2021-09-20T23:42:21.123456",
    "phoneNumber": "135987652543"
}

__CONST_Q5 = {
    "Route": "index",
    "Secret": "YjA1LTNiZDUtNCUyMDIxMDkwNDIyMTMxNV9fMzhfXzQ4X18xMDNfX2RlZmF1bHRfNzc1Yjg=",
    "TimeStamp": "2021-09-20T23:42:43.123456",
    "Type": "CustomEvent",
    "EventName": "clickButtonPayTrack",
    "User": {
              "FFUserName": "u_group0_0",
              "FFUserEmail": "u_group0_0@testliang.com",
              "FFUserCountry": "China",
              "FFUserKeyId": "u_group0_0@testliang.com",
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


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO,
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
    RabbitMQSender(mq_host,
                   mq_port,
                   mq_username,
                   mq_passwd,
                   redis_host,
                   redis_port,
                   redis_passwd).send('Q1', 'py.experiments.recordinginfo', __CONST_Q1_START)
    sleep(30)

    for i in range(100):
        # Q4
        RabbitMQSender(mq_host,
                       mq_port,
                       mq_username,
                       mq_passwd,
                       redis_host,
                       redis_port,
                       redis_passwd).send('Q4', 'py.experiments.events.ff', __CONST_Q4)
        # Q5
        RabbitMQSender(mq_host,
                       mq_port,
                       mq_username,
                       mq_passwd,
                       redis_host,
                       redis_port,
                       redis_passwd).send('Q5', 'py.experiments.events.user', __CONST_Q5)

    sleep(30)
    RabbitMQSender(mq_host,
                   mq_port,
                   mq_username,
                   mq_passwd,
                   redis_host,
                   redis_port,
                   redis_passwd).send('Q1', 'py.experiments.recordinginfo', __CONST_Q1_END)
