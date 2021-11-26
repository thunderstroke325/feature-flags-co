from datetime import datetime
from random import choice
from azure_service_bus.send_consume import AzureSender
from config.config_handling import get_config_value
from azure.servicebus import ServiceBusClient
import logging

logger = logging.getLogger('azure_simulator_start_expt')
logger.setLevel(logging.INFO)

if __name__ == '__main__':
    logging.basicConfig(level=logging.ERROR,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    redis_host = get_config_value('redis', 'redis_host')
    redis_port = get_config_value('redis', 'redis_port')
    redis_passwd = get_config_value('redis', 'redis_passwd')
    topic_1 = get_config_value('p1', 'topic_Q1')
    origin_1 = get_config_value('p1', 'subscription_Q1')
    topic_4 = get_config_value('p3', 'topic_Q4')
    origin_4 = get_config_value('p3', 'subscription_Q4')
    topic_5 = get_config_value('p3', 'topic_Q5')
    origin_5 = get_config_value('p3', 'subscription_Q5')
    sb_conn_str = get_config_value('azure', 'sb_conn_str')

    bus = ServiceBusClient.from_connection_string(conn_str=sb_conn_str, logging_enable=True)
    with bus:
        # Expt1
        # Q1 start
        expts = []
        for expt_num in range(1, 51):
            Q1_START = {
                "ExptId": f"FF__38__48__103__PayButton_{expt_num}_exp{expt_num}",
                "IterationId": "2",
                "EnvId": "103",
                "FlagId": f"FF__38__48__103__PayButton_{expt_num}",
                "BaselineVariation": "1",
                "Variations": ["1", "2", "3"],
                "EventName": f"ButtonPayTrack_{expt_num}",
                'EventType': 1,
                'CustomEventTrackOption': 1,
                'CustomEventSuccessCriteria': 1,
                'CustomEventUnit': None,
                "StartExptTime": "2021-09-20T21:00:00.123456",
                "EndExptTime": ""
            }
            expts.append(Q1_START)
        sender = AzureSender(None, redis_host, redis_port, redis_passwd)
        sender.send(*expts, bus=bus, topic=topic_1, subscription=origin_1)
        logger.info('send to Q1 expt start')
        for expt_num in range(1, 51):
            for group in range(1, 4):
                events = []
                for user in range(200):
                    # Q4
                    Q4 = {
                        "RequestPath": "index/paypage",
                        "FeatureFlagId": f"FF__38__48__103__PayButton_{expt_num}",
                        "EnvId": "103",
                        "AccountId": "38",
                        "ProjectId": "48",
                        "FeatureFlagKeyName": f"PayButton_{expt_num}",
                        "UserKeyId": "u_group" + str(group) + "_" + str(user) + "@testliang.com",
                        "FFUserName": "u_group" + str(group) + "_" + str(user),
                        "VariationLocalId": str(group),
                        "VariationValue": "Small-Button",
                        "TimeStamp": datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f"),
                        "phoneNumber": "135987652543"
                    }
                    events.append(Q4)
                sender.send(*events, bus=bus, topic=topic_4, subscription=origin_4)

            for group in range(1, 4):
                events = []
                weight = choice([i for i in range(10, 60)])
                for user in range(200 - weight * group):
                    Q5 = {
                        "Route": "index",
                        "Secret": "YjA1LTNiZDUtNCUyMDIxMDkwNDIyMTMxNV9fMzhfXzQ4X18xMDNfX2RlZmF1bHRfNzc1Yjg=",
                        "TimeStamp": datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f"),
                        "Type": "CustomEvent",
                        "EventName": f"ButtonPayTrack_{expt_num}",
                        "NumericValue": 1,
                        "User": {
                                "FFUserName": "u_group" + str(group) + "_" + str(user),
                                "FFUserEmail": "u_group" + str(group) + "_" + str(user) + "@testliang.com",
                                "FFUserCountry": "China",
                                "FFUserKeyId": "u_group" + str(group) + "_" + str(user) + "@testliang.com",
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
                    events.append(Q5)
                sender.send(*events, bus=bus, topic=topic_5, subscription=origin_5)

        sender.clear()
