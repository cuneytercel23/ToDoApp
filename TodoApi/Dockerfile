# See https://aka.ms/customizecontainer to learn how to customize your debug container
# and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ===============================
# Base runtime image
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ===============================
# Build image
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the csproj first (for better build cache)
COPY ["TodoApi/TodoApi.csproj", "TodoApi/"]

# Restore dependencies
RUN dotnet restore "TodoApi/TodoApi.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/TodoApi"
RUN dotnet build "TodoApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===============================
# Publish image
# ===============================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TodoApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===============================
# Final runtime image
# ===============================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoApi.dll"]
