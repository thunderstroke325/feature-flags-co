import logging
from redismq.send_consume import RedisSender

TOPIC_NAME = 'ds'
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
    sender = RedisSender()
    sender.send(*[Q1_START for _ in range(10)], topic=TOPIC_NAME)
