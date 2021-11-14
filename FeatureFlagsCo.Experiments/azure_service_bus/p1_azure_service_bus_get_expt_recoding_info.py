from experiment.generic_p1 import P1GetExptRecordingInfo

from azure_service_bus.send_consume import AzureReceiver


class P1AzureGetExptRecordingInfoReceiver(AzureReceiver, P1GetExptRecordingInfo):
    pass
