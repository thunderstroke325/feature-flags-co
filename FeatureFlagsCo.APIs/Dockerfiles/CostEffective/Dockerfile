# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build

ARG NODE_ENV ""

WORKDIR /source


# copy csproj and restore as distinct layers
COPY *.sln .
COPY FeatureFlags.APIs/*.csproj ./FeatureFlags.APIs/
COPY FeatureFlagsCo.MQ/*.csproj ./FeatureFlagsCo.MQ/
COPY FeatureFlagsCo.MQ.DirectExporter/*.csproj ./FeatureFlagsCo.MQ.DirectExporter/
COPY FeatureFlagsCo.APIs.Tests/*.csproj ./FeatureFlagsCo.APIs.Tests/
COPY FeatureFlagsCo.RabbitMQToGrafanaLoki/*.csproj ./FeatureFlagsCo.RabbitMQToGrafanaLoki/
RUN dotnet restore

# copy everything else and build app
COPY FeatureFlags.APIs/. ./FeatureFlags.APIs/
COPY FeatureFlagsCo.MQ/. ./FeatureFlagsCo.MQ/
COPY FeatureFlagsCo.MQ.DirectExporter/. ./FeatureFlagsCo.MQ.DirectExporter/
COPY FeatureFlagsCo.APIs.Tests/. ./FeatureFlagsCo.APIs.Tests/
COPY FeatureFlagsCo.RabbitMQToGrafanaLoki/. ./FeatureFlagsCo.RabbitMQToGrafanaLoki/
WORKDIR /source/FeatureFlags.APIs
RUN dotnet publish -c release -o /app --no-restore


RUN echo ${NODE_ENV}
ENV NODE_ENV ${NODE_ENV}

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5001   ASPNETCORE_ENVIRONMENT=DockerCostEffective
EXPOSE 5001
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "FeatureFlags.APIs.dll"]