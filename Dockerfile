# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
# If your project is in a folder named HealthyHabitsTracker, keep this:
COPY HealthyHabitsTracker/*.csproj HealthyHabitsTracker/

# Restore
RUN dotnet restore

# Copy the rest
COPY . .

# Publish
WORKDIR /src/HealthyHabitsTracker
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Render routes traffic to $PORT inside the container; we'll listen on 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# (Optional but recommended behind a proxy like Render)
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","HealthyHabitsTracker.dll"]
