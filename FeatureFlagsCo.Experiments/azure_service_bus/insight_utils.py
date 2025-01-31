
import logging
import os

from config.config_handling import get_config_value
from opencensus.ext.azure.log_exporter import AzureLogHandler


def get_insight_logger(name=os.path.basename(__file__)):
    logger = logging.getLogger(name)
    logger.addHandler(AzureLogHandler(connection_string=get_config_value('azure', 'insignt_conn_str')))
    return logger
