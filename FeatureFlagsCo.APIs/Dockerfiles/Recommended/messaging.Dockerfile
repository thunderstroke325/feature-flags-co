# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /source


# copy csproj and restore as distinct layers
COPY *.sln .
COPY FeatureFlags.APIs/*.csproj ./FeatureFlags.APIs/
COPY FeatureFlagsCo.MQ/*.csproj ./FeatureFlagsCo.MQ/
COPY FeatureFlagsCo.MQ.DirectExporter/*.csproj ./FeatureFlagsCo.MQ.DirectExporter/
COPY FeatureFlagsCo.APIs.Tests/*.csproj ./FeatureFlagsCo.APIs.Tests/
# COPY FeatureFlagsCo.RabbitMQToGrafanaLoki/*.csproj ./FeatureFlagsCo.RabbitMQToGrafanaLoki/
COPY FeatureFlagsCo.FeatureInsights/*.csproj ./FeatureFlagsCo.FeatureInsights/
COPY FeatureFlagsCo.FeatureInsights.ElasticSearch/*.csproj ./FeatureFlagsCo.FeatureInsights.ElasticSearch/
COPY FeatureFlagsCo.Export/*.csproj ./FeatureFlagsCo.Export/
COPY FeatureFlagsCo.Messaging/*.csproj ./FeatureFlagsCo.Messaging/

RUN dotnet restore


# copy everything else and build app
COPY FeatureFlags.APIs/. ./FeatureFlags.APIs/
COPY FeatureFlagsCo.MQ/. ./FeatureFlagsCo.MQ/
COPY FeatureFlagsCo.MQ.DirectExporter/. ./FeatureFlagsCo.MQ.DirectExporter/
COPY FeatureFlagsCo.APIs.Tests/. ./FeatureFlagsCo.APIs.Tests/
# COPY FeatureFlagsCo.RabbitMQToGrafanaLoki/. ./FeatureFlagsCo.RabbitMQToGrafanaLoki/
COPY FeatureFlagsCo.FeatureInsights/. ./FeatureFlagsCo.FeatureInsights/
COPY FeatureFlagsCo.FeatureInsights.ElasticSearch/. ./FeatureFlagsCo.FeatureInsights.ElasticSearch/
COPY FeatureFlagsCo.Export/. ./FeatureFlagsCo.Export/
COPY FeatureFlagsCo.Messaging/. ./FeatureFlagsCo.Messaging/


WORKDIR /source/FeatureFlagsCo.Messaging
RUN dotnet publish -c release -o /app --no-restore


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:6501  ASPNETCORE_ENVIRONMENT=DockerRecommended
EXPOSE 6501
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "FeatureFlagsCo.Messaging.dll"]