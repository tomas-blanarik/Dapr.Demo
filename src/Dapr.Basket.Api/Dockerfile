#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Dapr.Core/Dapr.Core.csproj", "Dapr.Core/"]
COPY ["Dapr.Basket.Api/Dapr.Basket.Api.csproj", "Dapr.Basket.Api/"]
RUN dotnet restore "Dapr.Basket.Api/Dapr.Basket.Api.csproj"
COPY . .
WORKDIR "/src/Dapr.Basket.Api"
RUN dotnet build "Dapr.Basket.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dapr.Basket.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Dapr.Basket.Api.dll"]