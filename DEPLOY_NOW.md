# Deploy to Railway - Step by Step Guide

Follow these steps exactly to deploy your Calendar App to Railway.

## Prerequisites Checklist

Before you start, make sure you have:
- [ ] Railway account (sign up at https://railway.app)
- [ ] GitHub account
- [ ] Gmail account with 2FA enabled
- [ ] Gmail App Password generated

---

## Part 1: GitHub Setup (10 minutes)

### Step 1: Create GitHub Repository

1. Go to https://github.com/new
2. Repository name: `calendar-app` (or your preferred name)
3. Description: "Full-stack calendar application with RabbitMQ messaging"
4. Privacy: Public or Private (your choice)
5. **DO NOT** initialize with README, .gitignore, or license
6. Click "Create repository"

### Step 2: Push Your Code to GitHub

Open your terminal in the project directory and run these commands:

```bash
# Initialize git repository
git init

# Add all files
git add .

# Create first commit
git commit -m "Initial commit - Calendar App with RabbitMQ integration"

# Add your GitHub repository as remote (REPLACE WITH YOUR URL)
git remote add origin https://github.com/YOUR_USERNAME/calendar-app.git

# Push to GitHub
git branch -M main
git push -u origin main
```

**Replace `YOUR_USERNAME/calendar-app` with your actual GitHub repository URL!**

✅ **Checkpoint**: Visit your GitHub repository URL - you should see all your files.

---

## Part 2: Gmail App Password (5 minutes)

### Step 1: Enable 2-Factor Authentication

1. Go to https://myaccount.google.com/security
2. Scroll to "2-Step Verification"
3. Follow the prompts to enable it (if not already enabled)

### Step 2: Generate App Password

1. Go to https://myaccount.google.com/apppasswords
2. In "Select app" dropdown: Choose "Mail"
3. In "Select device" dropdown: Choose "Other (Custom name)"
4. Type: "Calendar App"
5. Click "Generate"
6. **COPY THE 16-CHARACTER PASSWORD** (looks like: `xxxx xxxx xxxx xxxx`)
7. Save it somewhere safe - you'll need it multiple times

✅ **Checkpoint**: You have a 16-character Gmail App Password saved.

---

## Part 3: CloudAMQP Setup (5 minutes)

### Step 1: Create CloudAMQP Account

1. Go to https://www.cloudamqp.com
2. Click "Sign Up" or "Get Started"
3. Sign up with email or GitHub

### Step 2: Create RabbitMQ Instance

1. Click "Create New Instance"
2. **Name**: CalendarApp
3. **Plan**: Little Lemur (Free)
4. **Data center**: Choose closest to your location
5. Click "Select Region"
6. Review and click "Create instance"

### Step 3: Get Connection Details

1. Click on your "CalendarApp" instance
2. Copy the **AMQP URL** (looks like: `amqp://username:password@host.cloudamqp.com/vhost`)
3. Parse it into parts:
   - **Host**: `host.cloudamqp.com` (everything between @ and /)
   - **Username**: `username` (between amqp:// and :)
   - **Password**: `password` (between : and @)

**Example**:
```
AMQP URL: amqp://abc:def123@tiger.cloudamqp.com/ghi
Host: tiger.cloudamqp.com
Username: abc
Password: def123
```

✅ **Checkpoint**: You have CloudAMQP Host, Username, and Password saved.

---

## Part 4: Railway Project Setup (5 minutes)

### Step 1: Create Railway Account

1. Go to https://railway.app
2. Click "Login" or "Start a New Project"
3. Sign in with GitHub

### Step 2: Create New Project

1. Click "New Project"
2. Select "Deploy from GitHub repo"
3. If asked, authorize Railway to access your GitHub
4. Find and select your `calendar-app` repository
5. Railway will create an empty project

### Step 3: Add PostgreSQL Database

1. In your Railway project, click "+ New"
2. Select "Database"
3. Choose "Add PostgreSQL"
4. Railway will provision a PostgreSQL database
5. Click on the PostgreSQL service
6. Go to "Variables" tab
7. Copy the `DATABASE_URL` value (you'll need this)

✅ **Checkpoint**: You have a Railway project with PostgreSQL database.

---

## Part 5: Deploy API Service (15 minutes)

### Step 1: Create API Service

1. In Railway project, click "+ New"
2. Select "GitHub Repo"
3. Choose your repository
4. Railway creates a service (it might start building automatically)

### Step 2: Configure API Service

1. Click on the service Railway created
2. Click "Settings" tab
3. Under "Service Name", change to: `api`
4. Under "Root Directory", set to: `CalendarApp.API`
5. Under "Custom Build Command", leave empty (uses Dockerfile)
6. Under "Custom Start Command", leave empty (uses Dockerfile)

### Step 3: Add Environment Variables

1. Click "Variables" tab
2. Click "New Variable" for each of these:

**Database**:
```
ConnectionStrings__DefaultConnection
```
Value: Click "Add Reference" → PostgreSQL → DATABASE_URL

**RabbitMQ** (use your CloudAMQP values):
```
RabbitMQ__Host = tiger.cloudamqp.com
RabbitMQ__Port = 5672
RabbitMQ__Username = your-cloudamqp-username
RabbitMQ__Password = your-cloudamqp-password
```

**JWT** (generate a strong secret):
```
Jwt__SecretKey = YourSuperSecretKey_ChangeThis_MakeItLongAndRandom12345678901234567890
Jwt__Issuer = CalendarApp.API
Jwt__Audience = CalendarApp.Client
Jwt__ExpiryMinutes = 15
Jwt__RefreshTokenExpiryDays = 7
```

**Email** (use your Gmail App Password):
```
Email__SmtpHost = smtp.gmail.com
Email__SmtpPort = 587
Email__SmtpUsername = your-email@gmail.com
Email__SmtpPassword = your-16-char-app-password
Email__FromEmail = your-email@gmail.com
Email__FromName = Calendar App
```

**ASP.NET**:
```
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://0.0.0.0:$PORT
```

### Step 4: Generate Public Domain

1. Click "Settings" tab
2. Scroll to "Networking"
3. Click "Generate Domain"
4. Copy the domain (looks like: `api-production-xxxx.up.railway.app`)
5. **SAVE THIS URL** - you'll need it for frontend

### Step 5: Deploy

1. Railway should auto-deploy
2. Click "Deployments" tab to watch progress
3. Wait for deployment to complete (green checkmark)
4. Click on deployment to see logs

✅ **Checkpoint**: API deployed successfully, you have the API URL.

---

## Part 6: Deploy Message Consumer (10 minutes)

### Step 1: Create Consumer Service

1. In Railway project, click "+ New"
2. Select "GitHub Repo"
3. Choose your repository again

### Step 2: Configure Consumer Service

1. Click on the new service
2. Click "Settings"
3. Service Name: `consumer`
4. Root Directory: `CalendarApp.MessageConsumer`

### Step 3: Add Environment Variables

Same RabbitMQ and Email variables as API:

**RabbitMQ**:
```
RabbitMQ__Host = tiger.cloudamqp.com
RabbitMQ__Port = 5672
RabbitMQ__Username = your-cloudamqp-username
RabbitMQ__Password = your-cloudamqp-password
```

**Email**:
```
Email__SmtpHost = smtp.gmail.com
Email__SmtpPort = 587
Email__SmtpUsername = your-email@gmail.com
Email__SmtpPassword = your-16-char-app-password
Email__FromEmail = your-email@gmail.com
Email__FromName = Calendar App
```

### Step 4: Deploy

1. Railway auto-deploys
2. Watch "Deployments" tab
3. Check logs for "Consuming messages from queue"

✅ **Checkpoint**: Consumer deployed and running.

---

## Part 7: Deploy Frontend (10 minutes)

### Step 1: Create Frontend Service

1. In Railway project, click "+ New"
2. Select "GitHub Repo"
3. Choose your repository

### Step 2: Configure Frontend Service

1. Click on the service
2. Settings tab:
   - Service Name: `frontend`
   - Root Directory: `calendar-frontend`

### Step 3: Add Environment Variable

**IMPORTANT**: Use your API domain from Part 5, Step 4:

```
VITE_API_BASE_URL = https://api-production-xxxx.up.railway.app/api
```

Make sure to:
- Use `https://`
- Include `/api` at the end

### Step 4: Generate Public Domain

1. Settings → Networking → Generate Domain
2. Copy the frontend URL (like: `frontend-production-xxxx.up.railway.app`)

### Step 5: Update API CORS

1. Go back to API service
2. Variables tab
3. Add new variable:
```
AllowedOrigins__0 = https://frontend-production-xxxx.up.railway.app
```
Use your actual frontend URL!

4. This will trigger API redeployment

✅ **Checkpoint**: All three services deployed!

---

## Part 8: Testing (10 minutes)

### Step 1: Access Your Application

1. Open your frontend URL in browser: `https://frontend-production-xxxx.up.railway.app`
2. You should see the Calendar App login page

### Step 2: Test Registration

1. Click "Register" or "Sign Up"
2. Fill in:
   - Username: testuser
   - Email: your-email@gmail.com (use your real email!)
   - Password: Test123!
3. Click Register
4. Should redirect to login immediately

### Step 3: Check Welcome Email

1. Check your email inbox (might take 30-60 seconds)
2. Look for welcome email from Calendar App
3. Check spam folder if not in inbox

### Step 4: Test Login

1. Login with credentials you just created
2. Should see the calendar interface

### Step 5: Test Calendar Sharing

1. Click "Share" button
2. Enter another email address (can be another of your emails)
3. Click "Share Calendar"
4. Check that email inbox for sharing notification

### Step 6: Monitor Services

**Check Logs**:
1. Railway → API service → Deployments → Latest → View Logs
2. Look for "Database migrations completed successfully"
3. Check for any errors

**Check Consumer**:
1. Railway → Consumer service → Deployments → Latest → View Logs
2. Look for "Consuming messages from queue"
3. Look for "Welcome email sent to..."

**Check RabbitMQ**:
1. CloudAMQP dashboard → Your instance → RabbitMQ Manager
2. Click "Queues" tab
3. Should see `user-registered` and `calendar-shared` queues
4. Check message counts

✅ **All Tests Passed**: Your app is fully deployed and functional!

---

## Troubleshooting

### API Won't Start

**Check**:
1. Railway → API → Deployments → View Logs
2. Look for error messages

**Common Issues**:
- Missing environment variables
- Database connection failed
- RabbitMQ connection failed

**Fix**:
1. Verify all environment variables are set
2. Check `DATABASE_URL` reference is correct
3. Verify CloudAMQP credentials

### Emails Not Sending

**Check**:
1. Railway → Consumer → Deployments → View Logs
2. Look for email-related errors

**Common Issues**:
- Gmail App Password incorrect
- SMTP blocked by Gmail

**Fix**:
1. Regenerate Gmail App Password
2. Check Gmail security settings
3. Look for security alerts from Google

### Frontend Can't Connect to API

**Check**:
1. Browser console (F12) → Network tab
2. Look for CORS errors

**Common Issues**:
- VITE_API_BASE_URL incorrect
- CORS not configured
- API URL missing `/api`

**Fix**:
1. Verify `VITE_API_BASE_URL` includes `https://` and `/api`
2. Verify `AllowedOrigins__0` matches frontend URL exactly
3. Redeploy API after CORS change

### Database Migration Failed

**Check**:
1. API logs for "An error occurred while migrating the database"

**Fix**:
1. Ensure PostgreSQL is running
2. Check connection string format
3. May need to manually run migrations (advanced)

---

## Post-Deployment Checklist

- [ ] API service is running (green status)
- [ ] Consumer service is running (green status)
- [ ] Frontend service is running (green status)
- [ ] Can access frontend URL
- [ ] Can register a new user
- [ ] Received welcome email
- [ ] Can login successfully
- [ ] Can create tasks/todos
- [ ] Can share calendar
- [ ] Received sharing notification email
- [ ] All logs show no errors

---

## Your Railway URLs

After deployment, save these URLs:

```
Frontend: https://frontend-production-xxxx.up.railway.app
API: https://api-production-xxxx.up.railway.app
API Docs: https://api-production-xxxx.up.railway.app/swagger
```

---

## Cost Estimate

Railway Hobby Plan: $5/month
- Includes $5 usage credit
- Free tier: 500 hours/month
- Estimated cost for this app: $4-10/month

CloudAMQP Little Lemur: FREE
- Perfect for development and small production

---

## Need Help?

If you get stuck:

1. **Check logs** in Railway dashboard
2. **Review this guide** - step might have been missed
3. **Check [RAILWAY_DEPLOYMENT.md](RAILWAY_DEPLOYMENT.md)** for details
4. **Use [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md)**

---

**Estimated Total Time**: 60 minutes

**Ready? Let's deploy!** 🚀

Start with Part 1 and work through each section. Don't skip steps!
