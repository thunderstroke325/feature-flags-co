

from experiment.generic_p3 import P3GetExptFFEvents, P3GetExptUserEvents

from azure_service_bus.send_consume import AzureReceiver


class P3AzureGetExptFFEventsReceiver(AzureReceiver, P3GetExptFFEvents):
    pass


class P3AzureGetExptUserEventsReceiver(AzureReceiver, P3GetExptUserEvents):
    pass
