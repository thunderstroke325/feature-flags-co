FROM python:3.9
ARG exptenv=local
RUN apt update &&apt install -y nginx && apt install -y supervisor
EXPOSE 80
COPY . /expt
COPY ./env/docker-${exptenv}-config.ini /expt/config/config.ini
WORKDIR /expt
RUN pip install -r docker-requirements.txt
RUN mkdir -p /var/log/nginx
CMD ["/usr/bin/supervisord","-c","supervisord.conf"]