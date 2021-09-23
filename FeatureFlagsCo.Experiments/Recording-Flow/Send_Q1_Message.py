 #!/usr/bin/env python
import pika
import pickle
#Sending Message
credentials = pika.PlainCredentials('ffc-vm0-ce2', 'WonQijichuangtan30wanDollar')
parameters = pika.ConnectionParameters('52.131.239.36',
                                   5672,
                                   '/',
                                credentials)
connection = pika.BlockingConnection(parameters)
channel = connection.channel()
queue_name = 'ExptRecordingInfo'
channel.queue_declare(queue=queue_name)
message = {
	"EventName": "官网主页点击事件-",
    "Status" : "start",
	"Time": "2021-09-01T20:39:00",
	"Flag": {
		"Id": "FF__2__3__5__experimentation",
		"BaselineVariation": "1",
		"Variations": ["1", "2", "3", "4"]
	}
}
body_message = pickle.dumps(message)
channel.basic_publish(exchange='',
                  routing_key=queue_name,
                  body=body_message)
print(" [x] Sent Experiment Recording Information!")
connection.close()