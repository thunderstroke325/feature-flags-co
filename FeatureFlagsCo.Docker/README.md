# Architecture

![architecture](https://user-images.githubusercontent.com/68597908/130388173-dbdafb6f-49e5-4225-9f02-e1327bdcfde5.png)



# Run Feature-Flags.co as a Product with Docker Compose

You can also run the entire feature-flags-co with Docker by running the following commands:

    cd FeatureFlagsCo.Docker
    docker-compose -f docker-compose.yaml up

Before running commands above, please make sure you have configured projects as a Docker version.

1. In project FeatureFlags.APIs, set `ASPNETCORE_ENVIRONMENT` to `Local`. Right click on project "FeatureFlags.APIs" -> click on "Properties" -> choose tab "Debug" -> In "Environment variables" section, set `ASPNETCORE_ENVIRONMENT` to `Local` -> Save
2. In file `FeatureFlagsCo.Portal/src/environments/environment.standalone.ts`, make sure you have configured with values you desired. 

Here is an example (running in local) of configuration of file `FeatureFlagsCo.Portal/src/environments/environment.standalone.ts`

    export const environment = {
      production: false,  
      projectEnvKey: '',  
      url: 'http://localhost:5001',  // url of api service
      name: 'Standalone',
      statisticUrl: 'http://localhost:3000'   // url of grafana service
    };

Commands for remove and clearning your docker service:

    docker-compose -f docker-compose.yaml down
    docker rm -f $(docker ps -a -q)
    docker volume rm $(docker volume ls -q)


## mongodb 
https://newbedev.com/how-to-create-a-db-for-mongodb-container-on-start-up
### Account:
admin secret
### Installation
http://swarm-ip:8081, http://localhost:8081, or http://host-ip:8081

## rabbitmq
### Account:
guest guest
### Installation
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.9-management

## grafana 
### Account:
admin admin
### Installation
wget https://raw.githubusercontent.com/grafana/loki/v2.3.0/production/docker-compose.yaml -O docker-compose.yaml
### Some instruments
count_over_time({featureFlagId="ff__2__3__a1",varaition="false"}[30m])

sum(count_over_time({featureFlagId="ff__2__3__a1",varaition="false"}[30m])) by (userName)

count(count by (userName) (count_over_time({featureFlagId="ff__2__3__a1",varaition="false"}[1d])))

count(count by (userName) (count_over_time({featureFlagId="ff__2__3__a1"}[1d])))

{featureFlagId="ff__2__3__a1",varaition="false"}


## Sql Server

https://cardano.github.io/blog/2017/11/15/mssql-docker-container

If you encounter probleme like ".entrypoint.sh" not found. Please change file format from CRLF to LF. Because file will be executed in Ubuntu but we edited in windows. (https://stackoverflow.com/questions/29140377/sh-file-not-found)

### Account
sa YourSTRONG@Passw0rd


## Asp.Net Core
docker build -t featureflagscoapi .
docker run -d -p 5001:5001 --name featureflagscoapi featureflagscoapi

## RabbitMQToGrafanaLoki
docker build -t rabbitmq2grafanaloki -f Dockerfile-R2G .


# How to Do a Clean Restart of a Docker InstanceProcedure

Stop the container(s) using the following command: docker-compose down.
Delete all containers using the following command: docker rm -f $(docker ps -a -q)
Delete all volumes using the following command: docker volume rm $(docker volume ls -q)
Restart the containers using the following command: docker-compose up -d

