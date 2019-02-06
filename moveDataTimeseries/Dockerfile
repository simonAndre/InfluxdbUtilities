FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY moveDataTimeseries.csproj ./
RUN dotnet restore moveDataTimeseries.csproj
COPY . .
WORKDIR /src/
RUN dotnet build moveDataTimeseries.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish moveDataTimeseries.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

#Target for use with VSCode  - debug build
FROM build AS debug
WORKDIR /app
EXPOSE 7000


ENTRYPOINT ["dotnet", "moveDataTimeseries.dll"]
