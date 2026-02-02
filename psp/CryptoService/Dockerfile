FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY CryptoService/CryptoService.csproj CryptoService/

RUN dotnet restore CryptoService/CryptoService.csproj

COPY . .

WORKDIR /src/CryptoService
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "CryptoService.dll"]