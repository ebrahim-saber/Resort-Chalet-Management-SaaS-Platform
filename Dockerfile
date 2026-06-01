# Use .NET 9 SDK image as build environment
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy project files
COPY src/ResortManagement.Domain/ResortManagement.Domain.csproj src/ResortManagement.Domain/
COPY src/ResortManagement.Application/ResortManagement.Application.csproj src/ResortManagement.Application/
COPY src/ResortManagement.Infrastructure/ResortManagement.Infrastructure.csproj src/ResortManagement.Infrastructure/
COPY src/ResortManagement.WebApi/ResortManagement.WebApi.csproj src/ResortManagement.WebApi/

# Restore dependencies
RUN dotnet restore src/ResortManagement.WebApi/ResortManagement.WebApi.csproj

# Copy the entire source code
COPY src/ ./src/

# Build and publish release
RUN dotnet publish src/ResortManagement.WebApi/ResortManagement.WebApi.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ResortManagement.WebApi.dll"]
