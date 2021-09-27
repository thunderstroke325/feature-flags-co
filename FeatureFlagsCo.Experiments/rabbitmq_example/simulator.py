

import logging
from rabbitmq.rabbitmq import RabbitMQSender

__CONST_Q1_ = {"ExptId": 'FF__38__48__103__PayButton_exp1',
               "IterationId": 1,
               "EnvId": 103,
               "FlagId": "FF__38__48__103__PayButton",
               "BaselineVariation": "1",
               "Variations": ["1", "2", "3"],
               "EventName": "clickButtonPayTrack",
               "StartExptTime": "2021-09-20T21:00:00.123456",
               "EndExptTime": "2021-09-20T23:50:00.123456"}


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO,
                        format='%(asctime)s,%(msecs)d %(levelname)-8s [%(filename)s:%(lineno)d] %(message)s',
                        datefmt='%m-%d %H:%M')
    p1 = RabbitMQSender()
    p1.send('Q1', 'py.experiments.recordinginfo', __CONST_Q1_)
    logging.info('topic: %s, queue: %s sent' %
                 ('Q1', 'py.experiments.recordinginfo'))
