from azure_service_bus.send_consume import AzureSender
from config.config_handling import get_config_value
from azure.servicebus import ServiceBusClient
import logging

logger = logging.getLogger('azure_simulator_end_expt')
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
    sb_conn_str = get_config_value('azure', 'sb_conn_str')

    bus = ServiceBusClient.from_connection_string(conn_str=sb_conn_str, logging_enable=True)
    with bus:
        # Expt1
        #  Q1 end
        expts = []
        for expt_num in range(1, 11):
            Q1_END = {
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
                "EndExptTime": "2021-10-15T19:00:00.123456"
            }
            expts.append(Q1_END)

        sender = AzureSender(None, redis_host, redis_port, redis_passwd)
        sender.send(bus, topic_1, origin_1, *expts)
        sender.clear()
        logger.info('send to Q1 expt end')
