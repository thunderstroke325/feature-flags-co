from experiment.generic_p2 import P2GetExptResult

from azure_service_bus.send_consume import AzureReceiver


class P2AzureGetExptResultReceiver(AzureReceiver, P2GetExptResult):
    def __init__(self,
                 sb_host,
                 sb_sas_policy,
                 sb_sas_key,
                 redis_host='localhost',
                 redis_port='6379',
                 redis_passwd=None,
                 wait_timeout=30.0):
        super().__init__(sb_host, sb_sas_policy, sb_sas_key,
                         redis_host, redis_port, redis_passwd)
        self._init_wait_timeout(wait_timeout=wait_timeout)
