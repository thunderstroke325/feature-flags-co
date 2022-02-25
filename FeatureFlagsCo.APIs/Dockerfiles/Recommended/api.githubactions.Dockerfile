# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /source


# copy csproj and restore as distinct layers
COPY ./FeatureFlagsCo.APIs/*.sln .
COPY ./FeatureFlagsCo.APIs/FeatureFlags.Utils/*.csproj ./FeatureFlags.Utils/
COPY ./FeatureFlagsCo.APIs/FeatureFlags.APIs/*.csproj ./FeatureFlags.APIs/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.MQ/*.csproj ./FeatureFlagsCo.MQ/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.MQ.DirectExporter/*.csproj ./FeatureFlagsCo.MQ.DirectExporter/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.APIs.Tests/*.csproj ./FeatureFlagsCo.APIs.Tests/
# COPY FeatureFlagsCo.RabbitMQToGrafanaLoki/*.csproj ./FeatureFlagsCo.RabbitMQToGrafanaLoki/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.FeatureInsights/*.csproj ./FeatureFlagsCo.FeatureInsights/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.FeatureInsights.ElasticSearch/*.csproj ./FeatureFlagsCo.FeatureInsights.ElasticSearch/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.Export/*.csproj ./FeatureFlagsCo.Export/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.Messaging/*.csproj ./FeatureFlagsCo.Messaging/

RUN dotnet restore


# copy everything else and build app
COPY ./FeatureFlagsCo.APIs/FeatureFlags.Utils/. ./FeatureFlags.Utils/
COPY ./FeatureFlagsCo.APIs/FeatureFlags.APIs/. ./FeatureFlags.APIs/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.MQ/. ./FeatureFlagsCo.MQ/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.MQ.DirectExporter/. ./FeatureFlagsCo.MQ.DirectExporter/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.APIs.Tests/. ./FeatureFlagsCo.APIs.Tests/
# COPY FeatureFlagsCo.RabbitMQToGrafanaLoki/. ./FeatureFlagsCo.RabbitMQToGrafanaLoki/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.FeatureInsights/. ./FeatureFlagsCo.FeatureInsights/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.FeatureInsights.ElasticSearch/. ./FeatureFlagsCo.FeatureInsights.ElasticSearch/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.Export/. ./FeatureFlagsCo.Export/
COPY ./FeatureFlagsCo.APIs/FeatureFlagsCo.Messaging/. ./FeatureFlagsCo.Messaging/



WORKDIR /source/FeatureFlags.APIs
RUN dotnet publish -c release -o /app --no-restore


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5001  ASPNETCORE_ENVIRONMENT=DockerDevelopment
EXPOSE 5001
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "FeatureFlags.APIs.dll"]