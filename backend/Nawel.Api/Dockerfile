# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Nawel.Api.csproj", "./"]
RUN dotnet restore "Nawel.Api.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "Nawel.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Nawel.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create uploads directory for avatars
RUN mkdir -p /app/uploads/avatars

COPY --from=publish /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "Nawel.Api.dll"]
