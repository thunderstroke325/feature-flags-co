# Architecture

![architecture](https://user-images.githubusercontent.com/68597908/130388173-dbdafb6f-49e5-4225-9f02-e1327bdcfde5.png)


# Requirements of docker hosted system

|                  | CPU          | Memory |  Disk    | Ssytem                             |
| ---------------- | ------------ | ------ | -------- | ---------------------------------- |
| Cost-effective   | 2 * 1.30Ghz  | 4GB    |  128GB   | Ubuntu 18.04 / 20.04 (LTS) / 21.04 |
| Recommended      | 4 * 1.3Ghz   | 16GB   |  256GB   | Ubuntu 18.04 / 20.04 (LTS) / 21.04 |
| High Performance | X * 4.2Ghz   | 32GB+  |  512GB   | Ubuntu 18.04 / 20.04 (LTS) / 21.04 |

Run different docker commands for different case:

- Run Docker for Cost-effective solution
- Run Docker for Recommended solution
- Run Docker for local development
- Run Docker for local development (portal only)
- Run Docker for local development (API only)

## Run Docker for Cost-effective solution

Cost-effective solution provide an product level system. It means you can play with feature-flags-co without coding. This version use a async telemetry collector, this allows system can be running with low computing resources. Disadvantage is this version has doesn't support high concurrent requests.

You can run the entire feature-flags-co service with Docker by running the following commands:

    cd FeatureFlagsCo.Docker
    docker-compose --profile costeffective up


After docker compose started, please wait 30-50 secondes untill all services have been successfully etablished.

## Run Docker for Recommended solution

Recommended solution provide an product level system. This use an "InsightsExporter" to collect data and send data to a message queue instead of save directly to file/database.

You can run the entire feature-flags-co service with Docker by running the following commands:

    cd FeatureFlagsCo.Docker
    docker-compose --profile recommended up


After docker compose started, please wait 2-3 minutes untill all services have been successfully etablished. RabbitMQ take more times than other services, API services will run after the RabbitMQ started.

## Development on local

This solution will start all services except api, portal and rabbitmq.

You can run by following commands:

    cd FeatureFlagsCo.Docker
    docker-compose --profile development up

If you want to just develop app frontend portal, you can run by following commands:

    cd FeatureFlagsCo.Docker
    docker-compose --profile developmentportal up

This will start all services except portal and rabbitmq.

If you want to just develop api, you can run by following commands:

    cd FeatureFlagsCo.Docker
    docker-compose --profile developmentapi up

This will start all services except api and rabbitmq.


After docker compose started, please wait 30-50 secondes untill all services have been successfully etablished.


## Initial values

1. Portal Url: `http://localhost:4200`
2. API Url: `http://localhost:5001/swagger`
3. RabbitMQL Url: `http://localhost:15672/`. Default Username: `guest `, Default password: `guest`
4. Grafana Url: `http://localhost:3000`.  Default Username: `admin `, Default password: `admin`
5. Grafana Loki Url: `http://localhost:3100`.
6. Sql Server Express Url: `tcp://localhost,1433`. Default Username: `sa `, Default password: `YourSTRONG@Passw0rd`
7. MongoDB url: `mongodb://admin:password@localhost:27017/?authSource=admin&readPreference=primary&appname=MongoDB%20Compass&directConnection=true&ssl=false`

### Some instruments in Grafana

    count_over_time({featureFlagId="ff__2__3__a1",varaition="false"}[30m])
    sum(count_over_time({featureFlagId="ff__2__3__a1",varaition="false"}[30m])) by (userName)
    count(count by (userName) (count_over_time({featureFlagId="ff__2__3__a1",varaition="false"}[1d])))
    count(count by (userName) (count_over_time({featureFlagId="ff__2__3__a1"}[1d])))
    {featureFlagId="ff__2__3__a1",varaition="false"}

## Sql Server initilized issues

If you encounter probleme like ".entrypoint.sh" not found. Please change file format from CRLF to LF. Because file will be executed in Ubuntu but we edited in windows. (https://stackoverflow.com/questions/29140377/sh-file-not-found)

## How to Do a Clean Restart of a Docker InstanceProcedure

Stop the container(s) using the following command: docker-compose down.
Delete all containers using the following command: docker rm -f $(docker ps -a -q)
Delete all volumes using the following command: docker volume rm $(docker volume ls -q)
Restart the containers using the following command: docker-compose up -d


