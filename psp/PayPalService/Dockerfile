FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY PayPalService/PayPalService.csproj PayPalService/

RUN dotnet restore PayPalService/PayPalService.csproj

COPY . .

WORKDIR /src/PayPalService
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "PayPalService.dll"]