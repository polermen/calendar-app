# Railway Deployment Guide

This guide will walk you through deploying the Calendar App to Railway.

## Overview

Railway is a modern cloud platform that makes it easy to deploy full-stack applications. We'll deploy:
- Backend API (.NET 8)
- Message Consumer Worker
- Frontend (React + Vite)
- PostgreSQL Database
- RabbitMQ via CloudAMQP

## Prerequisites

1. Railway account (sign up at https://railway.app)
2. GitHub account (for connecting your repository)
3. Gmail account with App Password for sending emails

## Step-by-Step Deployment

### 1. Push Your Code to GitHub

If you haven't already, push your project to GitHub:

```bash
git init
git add .
git commit -m "Initial commit for Railway deployment"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO.git
git push -u origin main
```

### 2. Create a New Railway Project

1. Go to https://railway.app and sign in
2. Click "New Project"
3. Select "Deploy from GitHub repo"
4. Authorize Railway to access your GitHub account
5. Select your calendar app repository

### 3. Set Up PostgreSQL Database

1. In your Railway project, click "+ New"
2. Select "Database" → "Add PostgreSQL"
3. Railway will automatically provision a PostgreSQL database
4. Note: The `DATABASE_URL` environment variable is automatically set

### 4. Set Up RabbitMQ (CloudAMQP)

Railway doesn't have native RabbitMQ support, so we'll use CloudAMQP:

1. In your Railway project, click "+ New"
2. Select "Empty Service"
3. Name it "RabbitMQ"
4. Go to https://www.cloudamqp.com and sign up for a free account
5. Create a new instance:
   - Name: CalendarApp
   - Plan: Little Lemur (Free)
   - Region: Choose closest to your users
6. Once created, copy the **AMQP URL** (looks like: `amqp://user:pass@host/vhost`)
7. In Railway, add this to the "RabbitMQ" service variables:
   ```
   CLOUDAMQP_URL=amqp://user:pass@host/vhost
   ```

### 5. Deploy the Backend API

1. In your Railway project, click "+ New"
2. Select "GitHub Repo" and choose your repository
3. Name the service "API"
4. Configure the service:

#### Root Directory
Set to: `CalendarApp.API`

#### Build Configuration
- Builder: Dockerfile
- Dockerfile Path: `CalendarApp.API/Dockerfile`

#### Environment Variables

Click "Variables" and add the following:

```env
# Database Connection
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}

# RabbitMQ Connection (parse from CloudAMQP URL)
RabbitMQ__Host=your-cloudamqp-host.cloudamqp.com
RabbitMQ__Port=5672
RabbitMQ__Username=your-cloudamqp-username
RabbitMQ__Password=your-cloudamqp-password

# JWT Configuration
Jwt__SecretKey=YourSuperSecretKeyForJWTTokenGeneration123456789_ChangeThisInProduction
Jwt__Issuer=CalendarApp.API
Jwt__Audience=CalendarApp.Client
Jwt__ExpiryMinutes=15
Jwt__RefreshTokenExpiryDays=7

# Email Configuration (Gmail)
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__SmtpUsername=your-email@gmail.com
Email__SmtpPassword=your-gmail-app-password
Email__FromEmail=your-email@gmail.com
Email__FromName=Calendar App

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

**Important Notes:**
- Replace `your-cloudamqp-host`, `your-cloudamqp-username`, `your-cloudamqp-password` with values from your CloudAMQP URL
- Generate a new, secure JWT secret key for production
- Use a Gmail App Password (not your regular password)
- Railway automatically provides `$PORT` - don't change it

#### Parse CloudAMQP URL

The CloudAMQP URL format is:
```
amqp://username:password@hostname.cloudamqp.com/vhost
```

Extract:
- Host: `hostname.cloudamqp.com`
- Username: `username`
- Password: `password`

### 6. Deploy the Message Consumer

1. In your Railway project, click "+ New"
2. Select "GitHub Repo" and choose your repository
3. Name the service "Consumer"
4. Configure the service:

#### Root Directory
Set to: `CalendarApp.MessageConsumer`

#### Build Configuration
- Builder: Dockerfile
- Dockerfile Path: `CalendarApp.MessageConsumer/Dockerfile`

#### Environment Variables

```env
# RabbitMQ Connection (same as API)
RabbitMQ__Host=your-cloudamqp-host.cloudamqp.com
RabbitMQ__Port=5672
RabbitMQ__Username=your-cloudamqp-username
RabbitMQ__Password=your-cloudamqp-password

# Email Configuration (same as API)
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__SmtpUsername=your-email@gmail.com
Email__SmtpPassword=your-gmail-app-password
Email__FromEmail=your-email@gmail.com
Email__FromName=Calendar App
```

### 7. Deploy the Frontend

1. In your Railway project, click "+ New"
2. Select "GitHub Repo" and choose your repository
3. Name the service "Frontend"
4. Configure the service:

#### Root Directory
Set to: `calendar-frontend`

#### Build Configuration
- Builder: Nixpacks (auto-detected)
- Or use Dockerfile if you prefer

#### Environment Variables

```env
VITE_API_BASE_URL=https://your-api-service.up.railway.app/api
```

**Important:** Replace `your-api-service.up.railway.app` with the actual domain of your API service (found in Railway dashboard).

### 8. Configure PostgreSQL Connection String

Railway's PostgreSQL uses a different format than SQL Server. Update your API's connection string handling:

1. In the Railway dashboard, click on your PostgreSQL database
2. Copy the `DATABASE_URL` value
3. Your API should automatically use this via the environment variable

The format will be:
```
postgresql://username:password@hostname:port/database
```

You'll need to ensure your API can handle PostgreSQL. If it's currently using SQL Server, you'll need to:

#### Option A: Switch to PostgreSQL (Recommended for Railway)

1. Install Npgsql in your API project:
```bash
cd CalendarApp.API
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

2. Update `Program.cs`:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});
```

#### Option B: Use SQL Server on Azure

If you prefer to keep SQL Server, deploy it separately on Azure SQL Database and update the connection string.

### 9. Run Database Migrations

After deploying the API:

1. In Railway, click on your API service
2. Go to the "Deployments" tab
3. Click on the latest deployment
4. Open the "Deploy Logs" or "Runtime Logs"
5. You can run migrations by adding a startup script or manually via Railway CLI

**Automatic Migrations** (Add to `Program.cs` before `app.Run()`):

```csharp
// Auto-run migrations in production
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}
```

### 10. Configure CORS for Production

Update your API's `Program.cs` to allow your frontend domain:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://your-frontend-service.up.railway.app",
            "http://localhost:5173" // For local development
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
```

### 11. Set Up Custom Domains (Optional)

1. In Railway, click on a service
2. Go to "Settings"
3. Click "Generate Domain" for a free Railway subdomain
4. Or add your custom domain

### 12. Monitor Your Services

Railway provides built-in monitoring:

- **Metrics**: View CPU, memory, and network usage
- **Logs**: View real-time logs for each service
- **Deployments**: Track deployment history

#### Check RabbitMQ Messages

1. Go to CloudAMQP dashboard
2. Click on your instance
3. Go to "RabbitMQ Manager"
4. View queues: `user-registered`, `calendar-shared`

## Production Checklist

Before going live, ensure:

- [ ] Strong JWT secret key set (not the example one)
- [ ] Gmail App Password configured correctly
- [ ] All environment variables set on all services
- [ ] Database migrations completed successfully
- [ ] CORS configured for frontend domain
- [ ] Frontend points to correct API URL
- [ ] RabbitMQ queues are being consumed
- [ ] Test user registration (should send welcome email)
- [ ] Test calendar sharing (should send notification email)
- [ ] Check logs for any errors
- [ ] Test the application end-to-end

## Cost Estimate

Railway pricing (as of 2024):

- **Hobby Plan**: $5/month
  - $5 usage credit included
  - Pay only for what you use beyond that

- **Free Tier**:
  - 500 hours of service runtime per month
  - Shared CPU and memory

For this project:
- API: ~$2-5/month
- Consumer: ~$1-3/month
- Frontend: ~$1-2/month
- PostgreSQL: Included in Railway
- CloudAMQP: Free tier (suitable for development/small production)

**Total Estimated Cost**: $4-10/month (within Hobby plan)

## Troubleshooting

### API Won't Start

1. Check deployment logs in Railway
2. Ensure all environment variables are set
3. Check database connection string
4. Verify PostgreSQL is running

### Emails Not Sending

1. Check Consumer service logs
2. Verify Gmail App Password is correct
3. Check CloudAMQP dashboard for queued messages
4. Ensure Consumer service is running

### Frontend Can't Connect to API

1. Verify `VITE_API_BASE_URL` is correct
2. Check CORS configuration in API
3. Ensure API is deployed and running
4. Check API logs for errors

### Database Connection Issues

1. Verify `DATABASE_URL` environment variable
2. Check if migrations ran successfully
3. Ensure PostgreSQL service is healthy
4. Check connection string format

### RabbitMQ Connection Issues

1. Verify CloudAMQP credentials
2. Check if CloudAMQP instance is running
3. Ensure Host, Username, Password are correct
4. Test connection from CloudAMQP dashboard

## Updating Your Application

To deploy updates:

1. Push changes to GitHub:
```bash
git add .
git commit -m "Your update message"
git push
```

2. Railway will automatically detect the push and redeploy

## Rolling Back

If something goes wrong:

1. In Railway, click on the service
2. Go to "Deployments"
3. Find the previous working deployment
4. Click the three dots and select "Redeploy"

## Alternative: Railway CLI Deployment

You can also deploy using the Railway CLI:

```bash
# Install Railway CLI
npm i -g @railway/cli

# Login
railway login

# Link to your project
railway link

# Deploy a specific service
railway up --service api
railway up --service consumer
railway up --service frontend
```

## Support

- Railway Docs: https://docs.railway.app
- CloudAMQP Docs: https://www.cloudamqp.com/docs/index.html
- Railway Discord: https://discord.gg/railway

---

Congratulations! Your Calendar App is now deployed and fully functional on Railway.
