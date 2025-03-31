# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all project files first
COPY ["GOL.WebApi/GOL.WebApi.csproj", "GOL.WebApi/"]
COPY ["GOL.Application/GOL.Application.csproj", "GOL.Application/"]
COPY ["GOL.Infrastructure/GOL.Infrastructure.csproj", "GOL.Infrastructure/"]
COPY ["GOL.Domain/GOL.Domain.csproj", "GOL.Domain/"]

# Restore packages for all projects
RUN dotnet restore "GOL.WebApi/GOL.WebApi.csproj"

# Copy the rest of the code
COPY . .

# Build the project
WORKDIR "/src/GOL.WebApi"
RUN dotnet build "GOL.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GOL.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GOL.WebApi.dll"]