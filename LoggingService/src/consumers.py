import json
from celery import bootsteps
from kombu import Consumer, Exchange, Queue
from .mongo import init_db

logging_queue = Queue('logging', Exchange('logging', type="topic"), 'logging.#')


class LoggingConsumerStep(bootsteps.ConsumerStep):
    db = None

    def __init__(self, parent, **kwargs):
        super().__init__(parent, **kwargs)
        self.connect_db()

    def connect_db(self):
        self.db = init_db()

    def get_consumers(self, channel):
        return [Consumer(channel,
                         queues=[logging_queue],
                         callbacks=[self.handle_message],
                         accept=['json'])]

    def handle_message(self, body, message):
        body_data = json.loads(body, message.content_encoding)
        try:
            log_info = body_data['message']
        except KeyError:
            raise TypeError("Invalid message body format: no 'message' property.")
        log_type = '.'.join(message.delivery_info['routing_key'].split('.')[:2])
        try:
            save_data = json.loads(log_info, message.content_encoding)
        except json.JSONDecodeError:
            save_data = {"message": log_info}
        save_data["type"] = log_type
        self.db.logs.insert_one(save_data)
        message.ack()
