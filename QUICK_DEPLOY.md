# Quick Railway Deployment Guide

Your code is on GitHub! Now let's deploy it to Railway.

## What You'll Need (15 minutes to gather)

1. **Gmail App Password** - for sending emails
2. **CloudAMQP Account** - for RabbitMQ messaging
3. **Railway Account** - for hosting

---

## Step 1: Get Gmail App Password (5 min)

1. Go to: https://myaccount.google.com/apppasswords
2. You may need to enable 2-Factor Authentication first
3. Select:
   - App: "Mail"
   - Device: "Other" → Type "Calendar App"
4. Click "Generate"
5. **COPY THE 16-CHARACTER PASSWORD** (no spaces)
6. Save it - you'll use it multiple times

**Your Gmail**: nikolozge04@gmail.com
**App Password**: `________________` (16 characters)

---

## Step 2: Create CloudAMQP Account (5 min)

1. Go to: https://www.cloudamqp.com
2. Sign up (use GitHub for quick signup)
3. Click "Create New Instance"
4. Settings:
   - **Name**: CalendarApp
   - **Plan**: Little Lemur (Free)
   - **Region**: Pick closest to you
5. Click instance → Copy the **AMQP URL**

Example URL: `amqp://abc:pass123@tiger.cloudamqp.com/xyz`

Parse it:
- **Host**: `tiger.cloudamqp.com` (between @ and /)
- **Username**: `abc` (between amqp:// and :)
- **Password**: `pass123` (between : and @)

**Your CloudAMQP Details**:
```
Host: ________________
Username: ________________
Password: ________________
```

---

## Step 3: Deploy to Railway (30 min)

### A. Create Railway Account

1. Go to: https://railway.app
2. Click "Login with GitHub"
3. Authorize Railway

### B. Create New Project

1. Click "New Project"
2. Select "Deploy from GitHub repo"
3. Choose: `polermen/calendar-app`
4. Railway creates a project

### C. Add PostgreSQL Database

1. In project, click "+ New"
2. Select "Database" → "Add PostgreSQL"
3. Click PostgreSQL → "Variables" tab
4. **Copy** the `DATABASE_URL` value

### D. Deploy API Service

1. Click "+ New" → "GitHub Repo" → `polermen/calendar-app`
2. Railway may auto-detect and start building
3. Click on the service
4. Go to "Settings":
   - **Service Name**: Change to `api`
   - **Root Directory**: `CalendarApp.API`
   - **Start Command**: Leave empty (uses Dockerfile)

5. Go to "Variables" tab, add these ONE BY ONE:

**Database** (click "Add Reference" → PostgreSQL → DATABASE_URL):
```
ConnectionStrings__DefaultConnection = ${{Postgres.DATABASE_URL}}
```

**RabbitMQ** (use YOUR CloudAMQP details from Step 2):
```
RabbitMQ__Host = your-host.cloudamqp.com
RabbitMQ__Port = 5672
RabbitMQ__Username = your-username
RabbitMQ__Password = your-password
```

**JWT** (copy exactly):
```
Jwt__SecretKey = SuperSecretKey_ChangeThis_ProductionKey_12345678901234567890
Jwt__Issuer = CalendarApp.API
Jwt__Audience = CalendarApp.Client
Jwt__ExpiryMinutes = 15
Jwt__RefreshTokenExpiryDays = 7
```

**Email** (use YOUR Gmail App Password from Step 1):
```
Email__SmtpHost = smtp.gmail.com
Email__SmtpPort = 587
Email__SmtpUsername = nikolozge04@gmail.com
Email__SmtpPassword = your-16-char-app-password
Email__FromEmail = nikolozge04@gmail.com
Email__FromName = Calendar App
```

**Environment**:
```
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://0.0.0.0:$PORT
```

6. Wait for deployment to complete (watch "Deployments" tab)

7. Go to "Settings" → "Networking" → Click "Generate Domain"

8. **COPY YOUR API URL**: `https://api-production-XXXX.up.railway.app`

### E. Deploy Consumer Service

1. Click "+ New" → "GitHub Repo" → `polermen/calendar-app`
2. Click on the service
3. Settings:
   - **Service Name**: `consumer`
   - **Root Directory**: `CalendarApp.MessageConsumer`

4. Variables (same as API):

**RabbitMQ**:
```
RabbitMQ__Host = your-host.cloudamqp.com
RabbitMQ__Port = 5672
RabbitMQ__Username = your-username
RabbitMQ__Password = your-password
```

**Email**:
```
Email__SmtpHost = smtp.gmail.com
Email__SmtpPort = 587
Email__SmtpUsername = nikolozge04@gmail.com
Email__SmtpPassword = your-16-char-app-password
Email__FromEmail = nikolozge04@gmail.com
Email__FromName = Calendar App
```

5. Wait for deployment

### F. Deploy Frontend

1. Click "+ New" → "GitHub Repo" → `polermen/calendar-app`
2. Click on service
3. Settings:
   - **Service Name**: `frontend`
   - **Root Directory**: `calendar-frontend`

4. Variables (use YOUR API URL from step D.8):
```
VITE_API_BASE_URL = https://api-production-XXXX.up.railway.app/api
```
**IMPORTANT**: Include `/api` at the end!

5. Settings → Networking → "Generate Domain"

6. **COPY FRONTEND URL**: `https://frontend-production-XXXX.up.railway.app`

### G. Update API CORS

1. Go back to **API service**
2. Variables tab → Add:
```
AllowedOrigins__0 = https://frontend-production-XXXX.up.railway.app
```
Use YOUR actual frontend URL (no trailing slash)

3. This triggers API redeployment

---

## Step 4: Test Your App! (10 min)

1. **Open frontend**: `https://frontend-production-XXXX.up.railway.app`

2. **Register a user**:
   - Username: testuser
   - Email: nikolozge04@gmail.com
   - Password: Test123!

3. **Check email**: You should receive a welcome email!

4. **Login** with credentials

5. **Share calendar**:
   - Click "Share" button
   - Enter another email
   - That email should receive notification!

---

## Troubleshooting

### Can't access frontend
- Check if frontend service is deployed (green status)
- Check browser console (F12) for errors

### API errors
- Railway → API → Deployments → Latest → View Logs
- Look for "Database migrations completed successfully"

### Emails not sending
- Railway → Consumer → Deployments → View Logs
- Check Gmail App Password is correct
- Check spam folder

### CORS errors
- Verify `AllowedOrigins__0` matches frontend URL exactly
- Verify `VITE_API_BASE_URL` ends with `/api`

---

## Your Deployment URLs

Fill these in as you deploy:

```
Frontend: https://___________________________
API: https://___________________________
API Swagger: https://_________________________/swagger
```

---

## Estimated Costs

- **Railway**: $5/month (includes $5 credit, actual usage ~$4-10/month)
- **CloudAMQP**: FREE (Little Lemur plan)
- **Total**: ~$4-10/month

---

## Success Checklist

- [ ] All 3 services showing green status in Railway
- [ ] Can access frontend URL
- [ ] Can register new user
- [ ] Received welcome email
- [ ] Can login
- [ ] Can create tasks/todos
- [ ] Can share calendar
- [ ] Received sharing notification email

---

## Need Help?

See the detailed guides:
- [DEPLOY_NOW.md](DEPLOY_NOW.md) - Full step-by-step
- [RAILWAY_DEPLOYMENT.md](RAILWAY_DEPLOYMENT.md) - Complete Railway guide
- [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md) - Full checklist

---

**Time to complete**: ~50 minutes total

**Let's deploy!** 🚀
