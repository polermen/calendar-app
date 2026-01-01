# Docker Compose Setup Guide

This guide will help you run the Calendar App locally using Docker Compose.

## Prerequisites

- Docker Desktop installed and running
- .NET 8.0 SDK (for local development)
- Node.js 20+ (for local development)

## Quick Start

### 1. Configure Email Settings

Copy the `.env.example` file to `.env`:

```bash
cp .env.example .env
```

Edit the `.env` file and add your SMTP credentials:

```env
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
SMTP_FROM_EMAIL=your-email@gmail.com
```

**Note for Gmail users:**
- You need to use an App Password, not your regular Gmail password
- Enable 2-Factor Authentication on your Google account
- Generate an App Password at: https://myaccount.google.com/apppasswords
- Select "Mail" as the app and "Other" as the device
- Copy the 16-character password (without spaces)

### 2. Start All Services

Run the following command from the project root:

```bash
docker-compose up --build
```

This will start:
- **SQL Server** on port 1433
- **RabbitMQ** on ports 5672 (AMQP) and 15672 (Management UI)
- **API** on port 5000
- **Message Consumer** (background worker)
- **Frontend** on port 5173

### 3. Run Database Migrations

After all services are running, execute migrations in a new terminal:

```bash
docker-compose exec api dotnet ef database update
```

Or, if you prefer to run migrations from your local machine:

```bash
cd CalendarApp.API
dotnet ef database update
```

**Note**: The connection string for local migrations should be:
```
Server=localhost,1433;Database=CalendarAppDB;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;MultipleActiveResultSets=true
```

### 4. Access the Application

- **Frontend**: http://localhost:5173
- **API**: http://localhost:5000
- **RabbitMQ Management UI**: http://localhost:15672 (username: `guest`, password: `guest`)
- **API Swagger**: http://localhost:5000/swagger

## Service Architecture

```
┌─────────────┐
│  Frontend   │ :5173
│  (React)    │
└──────┬──────┘
       │
       │ HTTP
       ▼
┌─────────────┐     ┌──────────────┐
│  API        │────▶│  SQL Server  │ :1433
│  (.NET 8)   │     │  Database    │
└──────┬──────┘     └──────────────┘
       │
       │ Publish Messages
       ▼
┌─────────────┐
│  RabbitMQ   │ :5672, :15672
│  (Broker)   │
└──────┬──────┘
       │
       │ Consume Messages
       ▼
┌─────────────┐
│  Consumer   │
│  (Worker)   │────▶ Sends Emails
└─────────────┘
```

## Stopping Services

To stop all services:

```bash
docker-compose down
```

To stop and remove volumes (this will delete the database):

```bash
docker-compose down -v
```

## Debugging

### View Logs

View logs for all services:
```bash
docker-compose logs -f
```

View logs for a specific service:
```bash
docker-compose logs -f api
docker-compose logs -f consumer
docker-compose logs -f rabbitmq
```

### Check Service Health

```bash
docker-compose ps
```

### Access Container Shell

```bash
docker-compose exec api bash
docker-compose exec sqlserver bash
```

### Check RabbitMQ Queues

Visit the RabbitMQ Management UI at http://localhost:15672 and navigate to the "Queues" tab to see:
- `user-registered` queue
- `calendar-shared` queue

You can see message counts, publish/consume rates, and manually inspect messages.

## Troubleshooting

### API Cannot Connect to SQL Server

If you see connection errors, wait a few moments for SQL Server to fully start. The healthcheck ensures it's ready, but migrations may need a few extra seconds.

### RabbitMQ Connection Refused

Ensure RabbitMQ is fully started:
```bash
docker-compose logs rabbitmq
```

Look for the message: "Server startup complete"

### Emails Not Sending

1. Check your SMTP credentials in the `.env` file
2. View consumer logs: `docker-compose logs -f consumer`
3. Verify messages are being published to RabbitMQ via the Management UI
4. Check if Gmail is blocking the login (check your email for security alerts)

### Port Already in Use

If you get port conflicts, edit the `docker-compose.yml` file and change the port mappings:

```yaml
ports:
  - "5001:5000"  # Changed from 5000:5000
```

## Production Deployment

For deploying to Railway or other cloud platforms, see the [RAILWAY_DEPLOYMENT.md](RAILWAY_DEPLOYMENT.md) guide.

## Development Workflow

For local development without Docker:

1. Start only infrastructure services:
```bash
docker-compose up sqlserver rabbitmq
```

2. Run the API locally:
```bash
cd CalendarApp.API
dotnet run
```

3. Run the consumer locally:
```bash
cd CalendarApp.MessageConsumer
dotnet run
```

4. Run the frontend locally:
```bash
cd calendar-frontend
npm run dev
```

This approach provides faster rebuild times during development.
