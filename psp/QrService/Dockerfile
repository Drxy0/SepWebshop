FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY QrService.Domain/QrService.Domain.csproj QrService.Domain/
COPY QrService.Infrastructure/QrService.Infrastructure.csproj QrService.Infrastructure/
COPY QrService.Application/QrService.Application.csproj QrService.Application/
COPY QrService.API/QrService.API.csproj QrService.API/

RUN dotnet restore QrService.API/QrService.API.csproj

COPY . .

WORKDIR /src/QrService.API
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "QrService.API.dll"]