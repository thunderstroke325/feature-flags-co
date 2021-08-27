

# Requirements of docker hosted system

|                  | CPU          | Memory |  Disk    | Ssytem                             |
| ---------------- | ------------ | ------ | -------- | ---------------------------------- |
| Minimum          | 2 * 1.3Ghz   | 8GB    |  128GB   | Ubuntu 18.04 / 20.04 (LTS) / 21.04 |
| Recommended      | 4 * 1.3Ghz   | 16GB   |  256GB   | Ubuntu 18.04 / 20.04 (LTS) / 21.04 |
| High Performance | X * 4.2Ghz   | 32GB+  |  512GB   | Ubuntu 18.04 / 20.04 (LTS) / 21.04 |

Run different docker commands for different case:

- Run Docker for entire solution
- Run Docker for development solution

## Run Docker for entire solution

This solution provide an product level system. It means you can play with feature-flags-co without coding. 

You can run the entire feature-flags-co service with Docker by running the following commands:

    cd FeatureFlagsCo.Docker
    docker-compose --profile docker up


After docker compose started, please wait 30-50 secondes untill all services have been successfully etablished.

## Run Docker for development solution

This solution will start all services except api, portal.

You can run the entire feature-flags-co service with Docker by running the following commands:

    cd FeatureFlagsCo.Docker
    docker-compose --profile development up

After docker compose started, please wait 2-3 minutes untill all services have been successfully etablished. RabbitMQ take more times than other services, API services will run after the RabbitMQ started.

## Initial values

1. Portal Url: `http://localhost:4200`
2. API Url: `http://localhost:5001/swagger`
3. RabbitMQL Url: `http://localhost:15672/`. Default Username: `guest `, Default password: `guest`
4. Grafana Url: `http://localhost:3000`.  Default Username: `admin `, Default password: `admin`
5. Grafana Loki Url: `http://localhost:3100`.
6. Sql Server Express Url: `tcp://localhost,1433`. Default Username: `sa `, Default password: `YourSTRONG@Passw0rd`
7. MongoDB url: `mongodb://admin:password@localhost:27017/?authSource=admin&readPreference=primary&appname=MongoDB%20Compass&directConnection=true&ssl=false`
8. Elastic search Kibana: `http://localhost:5601`

## Sql Server initilized issues

If you encounter probleme like ".entrypoint.sh" not found. Please change file format from CRLF to LF. Because file will be executed in Ubuntu but we edited in windows. (https://stackoverflow.com/questions/29140377/sh-file-not-found)

## How to Do a Clean Restart of a Docker InstanceProcedure

Stop the container(s) using the following command: docker-compose down.
Delete all containers using the following command: docker rm -f $(docker ps -a -q)
Delete all volumes using the following command: docker volume rm $(docker volume ls -q)
Restart the containers using the following command: docker-compose up -d


