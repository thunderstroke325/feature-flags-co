FROM python:3.9-slim
ARG exptenv=local
RUN apt update && apt install -y supervisor
COPY . /expt
COPY ./env/docker-${exptenv}-config.ini /expt/config/config.ini
WORKDIR /expt
RUN pip install -r docker-requirements.txt
CMD ["/usr/bin/supervisord","-c","supervisord.conf"]