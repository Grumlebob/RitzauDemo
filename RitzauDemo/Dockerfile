# syntax=docker/dockerfile:1.24

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 7186

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS source
WORKDIR /src
COPY global.json ./
COPY Directory.Build.props ./
COPY Directory.Build.targets ./
COPY Directory.Packages.props ./
COPY RitzauDemo/RitzauDemo.csproj RitzauDemo/
COPY RitzauDemo.Client/RitzauDemo.Client.csproj RitzauDemo.Client/
COPY RitzauDemo.Core/RitzauDemo.Core.csproj RitzauDemo.Core/
COPY RitzauDemo/ RitzauDemo/
COPY RitzauDemo.Client/ RitzauDemo.Client/
COPY RitzauDemo.Core/ RitzauDemo.Core/

FROM source AS publish-blazor
ARG FRONTEND_PROFILE_OVERRIDE
RUN set -eu; \
    profile="${FRONTEND_PROFILE_OVERRIDE:-BlazorAuto}"; \
    test "$profile" = "BlazorAuto" || { echo "final-blazor requires FRONTEND_PROFILE_OVERRIDE=BlazorAuto (received '$profile')" >&2; exit 2; }; \
    dotnet publish ./RitzauDemo/RitzauDemo.csproj -c Release -o /app/publish /p:UseAppHost=false "-p:FrontendProfile=$profile"

FROM base AS final-blazor
LABEL com.ritzaudemo.frontend-profile="BlazorAuto" \
      com.ritzaudemo.acceptance-profile="BlazorAutoV1"
WORKDIR /app
COPY --from=publish-blazor --chown=app:app /app/publish .
USER app
ENTRYPOINT ["dotnet", "RitzauDemo.dll"]
