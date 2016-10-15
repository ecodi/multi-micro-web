from pymongo import ASCENDING, MongoClient


def init_db(_connect=True):
    client = MongoClient('localhost', 27017, _connect=_connect)
    db = client['multimicroweb']
    db.logs.create_index([('type', ASCENDING)], unique=False)
    return db
