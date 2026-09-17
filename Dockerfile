# =========================================================
# المرحلة 1: البناء
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AuthApi.API/AuthApi.API.csproj", "AuthApi.API/"]
COPY ["AuthApi.Application/AuthApi.Application.csproj", "AuthApi.Application/"]
COPY ["AuthApi.Domain/AuthApi.Domain.csproj", "AuthApi.Domain/"]
COPY ["AuthApi.Infrastructure/AuthApi.Infrastructure.csproj", "AuthApi.Infrastructure/"]

RUN dotnet restore "AuthApi.API/AuthApi.API.csproj"

COPY . .

WORKDIR /src/AuthApi.API
RUN dotnet publish "AuthApi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =========================================================
# المرحلة 2: التشغيل
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AuthApi.API.dll"]