from flask import Flask
from api.elastic_connection import connect_elasticsearch

app = Flask(__name__)

from api.insert_data import *
from api.retrieve_data import *
from api.test import * 

if __name__ == '__main__':
    app.run()