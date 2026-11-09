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
COPY frontend-profile.txt ./
COPY GrumPlates/GrumPlates.csproj GrumPlates/
COPY GrumPlates.Client/GrumPlates.Client.csproj GrumPlates.Client/
COPY GrumPlates.Core/GrumPlates.Core.csproj GrumPlates.Core/
COPY GrumPlates/ GrumPlates/
COPY GrumPlates.Client/ GrumPlates.Client/
COPY GrumPlates.Core/ GrumPlates.Core/

FROM source AS publish-blazor
ARG FRONTEND_PROFILE_OVERRIDE
RUN set -eu; \
    profile="${FRONTEND_PROFILE_OVERRIDE:-$(cat frontend-profile.txt)}"; \
    test "$profile" = "BlazorAuto" || { echo "final-blazor requires FRONTEND_PROFILE_OVERRIDE=BlazorAuto (received '$profile')" >&2; exit 2; }; \
    dotnet publish ./GrumPlates/GrumPlates.csproj -c Release -o /app/publish /p:UseAppHost=false "-p:FrontendProfile=$profile"

FROM base AS final-blazor
LABEL com.grumplates.frontend-profile="BlazorAuto" \
      com.grumplates.acceptance-profile="BlazorAutoV1"
WORKDIR /app
COPY --from=publish-blazor --chown=app:app /app/publish .
USER app
ENTRYPOINT ["dotnet", "GrumPlates.dll"]
