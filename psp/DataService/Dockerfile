FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY DataService/DataService.csproj DataService/

RUN dotnet restore DataService/DataService.csproj

COPY . .

WORKDIR /src/DataService
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "DataService.dll"]