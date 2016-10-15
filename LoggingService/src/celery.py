from celery import Celery
from .consumers import LoggingConsumerStep

app = Celery('logging')
app.config_from_object('src.celeryconfig')
app.steps['consumer'].add(LoggingConsumerStep)
