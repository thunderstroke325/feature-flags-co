FROM tiangolo/uwsgi-nginx-flask:python3.9
ENV UWSGI_CHEAPER 2
ENV UWSGI_PROCESSES 16
ENV NGINX_WORKER_PROCESSES auto
ENV LISTEN_PORT 80
EXPOSE 80
COPY . /app
RUN pip install -r docker-flask-requirements.txt