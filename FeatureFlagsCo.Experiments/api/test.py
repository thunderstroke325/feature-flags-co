from flask.wrappers import Response
from ExperimentApiPy import app


@app.route('/api/Test', methods=['GET'])
def test():
    return { "status": "ok"}
