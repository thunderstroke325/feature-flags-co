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

After docker compose started, please wait 2-3 minutes untill all services have been successfully etablished. RabbitMQ take more times than other services, API services will run after the RabbitMQ started.

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

# Requirements

## Minimum requirements

https://docs.contrastsecurity.com/en/-net-core-system-requirements.html


|             | vCPU        | Memory |  Disk    |
| Minimum     | 2 * 1.4Ghz | 4GB    |  128GB   |
| Recommended | 4 * 1.4Ghz | 16GB   |  256GB   |