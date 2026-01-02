# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["CalendarApp.MessageConsumer.csproj", "./"]
RUN dotnet restore "CalendarApp.MessageConsumer.csproj"

# Copy all source files
COPY . .

# Build the project
RUN dotnet build "CalendarApp.MessageConsumer.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CalendarApp.MessageConsumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Start the worker service
ENTRYPOINT ["dotnet", "CalendarApp.MessageConsumer.dll"]
