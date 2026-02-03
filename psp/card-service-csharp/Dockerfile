FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY CardService.Domain/CardService.Domain.csproj CardService.Domain/
COPY CardService.Infrastructure/CardService.Infrastructure.csproj CardService.Infrastructure/
COPY CardService.Application/CardService.Application.csproj CardService.Application/
COPY CardService.API/CardService.API.csproj CardService.API/

RUN dotnet restore CardService.API/CardService.API.csproj

COPY . .

WORKDIR /src/CardService.API
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "CardService.API.dll"]
