FROM python:3.9
ARG exptenv=local
RUN apt update &&apt install -y nginx && apt install -y supervisor
RUN mkdir -p /var/log/nginx && \
    touch /var/log/nginx/access.log && \
    touch /var/log/nginx/error.log
EXPOSE 80
COPY . /expt
COPY ./env/docker-${exptenv}-config.ini /expt/config/config.ini
WORKDIR /expt
RUN pip install -r docker-requirements.txt
CMD ["/usr/bin/supervisord","-c","supervisord.conf"]