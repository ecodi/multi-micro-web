import json
from unittest.mock import Mock
from pytest import fixture, mark, raises
from kombu.message import Message
from src.consumers import LoggingConsumerStep


class TestLoggingConsumerStep:
    @fixture
    def mongo_db_mock(self):
        class Logs:
            insert_one = Mock(wraps=lambda data: data)

        class MongoDb:
            logs = Logs()

        return MongoDb()

    @mark.parametrize("body, routing_key, expected_save_data", (
            ({"message": '{"attr1": "val1", "attr2": 3}'}, "some.routing.key.1234",
             {"attr1": "val1", "attr2": 3, "type": "some.routing"}),
            ({"message": "some plain text"}, "1.2.3",
             {"message": "some plain text", "type": "1.2"}),
            ({"message": '{}'}, "test-key", {"type": "test-key"}),
            ({"no_message": '{}'}, "test-key", None),
    ))
    def test_handle_message(self, monkeypatch, mongo_db_mock, body, routing_key, expected_save_data):
        body = json.dumps(body)
        monkeypatch.setattr(LoggingConsumerStep, "connect_db", Mock())
        logging_consumer = LoggingConsumerStep(None)
        logging_consumer.db = mongo_db_mock
        msg = Message(Mock(), body=body, delivery_info={'routing_key': routing_key})

        def tested_func():
            logging_consumer.handle_message(body, msg)

        if expected_save_data is not None:
            tested_func()
            mongo_db_mock.logs.insert_one.assert_called_once_with(expected_save_data)
        else:
            with raises(TypeError):
                tested_func()
