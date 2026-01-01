# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["CalendarApp.API/CalendarApp.API.csproj", "CalendarApp.API/"]
RUN dotnet restore "CalendarApp.API/CalendarApp.API.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/CalendarApp.API"
RUN dotnet build "CalendarApp.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CalendarApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 5000

# Copy published files
COPY --from=publish /app/publish .

# Run migrations and start the application
ENTRYPOINT ["dotnet", "CalendarApp.API.dll"]
