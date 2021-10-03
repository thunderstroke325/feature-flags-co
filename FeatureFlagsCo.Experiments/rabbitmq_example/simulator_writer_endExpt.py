

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

    # Expt1
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
        "EndExptTime": "2021-10-03T21:00:00.123456"
    }
    RabbitMQSender(mq_host,
                   mq_port,
                   mq_username,
                   mq_passwd,
                   redis_host,
                   redis_port,
                   redis_passwd).send('Q1', 'py.experiments.recordinginfo', Q1_END)
    logger.info('send to Q1 expt end')

    # Expt2
    #  Q1 end
    Q1_END = {
        "ExptId": 'FF__38__48__103__PayColor_exp2',
        "IterationId": "2",
        "EnvId": "103",
        "FlagId": "FF__38__48__103__PayColor",
        "BaselineVariation": "1",
        "Variations": ["1", "2", "3"],
        "EventName": "ButtonPayColor",
        "StartExptTime": "2021-09-20T21:00:00.123456",
        "EndExptTime": "2021-10-03T21:00:00.123456"
    }
    RabbitMQSender(mq_host,
                   mq_port,
                   mq_username,
                   mq_passwd,
                   redis_host,
                   redis_port,
                   redis_passwd).send('Q1', 'py.experiments.recordinginfo', Q1_END)
    logger.info('send to Q1 expt end')