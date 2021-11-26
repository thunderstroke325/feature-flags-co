
# FROM nginx:latest
# RUN apt-get update && apt-get install -y \
#     software-properties-common \
#     npm
# RUN npm install npm@latest -g && \
#     npm install n -g && \
#     n latest

# Above is for using nginx:latest (ubuntu)

FROM nginx:1.21.1-alpine

ARG API_URL=https://api.feature-flags.co

RUN apk add --update nodejs npm

# Above is for using alpine

# The following code is common for both nginx:latest and alpine
RUN npm install -g @angular/cli

# Bundle app source
COPY . /src

RUN sed -i "s|#{API_URL}#|${API_URL}|g" /src/src/environments/environment.standalone.ts

RUN cd /src; npm install --legacy-peer-deps; ng build --configuration standalone

RUN cp -r /src/dist/feature-flagsco-fontend/. /usr/share/nginx/html/
COPY nginx.conf /etc/nginx/conf.d/default.conf

RUN rm -rf /src

EXPOSE 80
