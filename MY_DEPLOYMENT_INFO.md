# My Deployment Information

## ✅ Step 1: Gmail Configuration (DONE)

**Email**: mrpolermen@gmail.com
**App Password**: klsp uqjo aquh ywlc

**Environment Variables for Railway**:
```
Email__SmtpHost = smtp.gmail.com
Email__SmtpPort = 587
Email__SmtpUsername = mrpolermen@gmail.com
Email__SmtpPassword = klspuqjoaquhywlc
Email__FromEmail = mrpolermen@gmail.com
Email__FromName = Calendar App
```

---

## ✅ Step 2: CloudAMQP Setup (DONE)

**AMQP URL**: `amqps://sqoaihpc:jsPTiumGAKlcG_uApOniU4P4Aut77PEE@ostrich.lmq.cloudamqp.com/sqoaihpc`

Parse it:
- **Host**: `ostrich.lmq.cloudamqp.com`
- **Username**: `sqoaihpc`
- **Password**: `jsPTiumGAKlcG_uApOniU4P4Aut77PEE`

**Environment Variables for Railway**:
```
RabbitMQ__Host = ostrich.lmq.cloudamqp.com
RabbitMQ__Port = 5672
RabbitMQ__Username = sqoaihpc
RabbitMQ__Password = jsPTiumGAKlcG_uApOniU4P4Aut77PEE
```

---

## Step 3: Railway Deployment (TODO)

### GitHub Repository (DONE)
✅ https://github.com/polermen/calendar-app

### A. PostgreSQL Database
1. Railway → New Project → Deploy from GitHub → `polermen/calendar-app`
2. Click "+ New" → Database → PostgreSQL
3. Click PostgreSQL → Variables → Copy `DATABASE_URL`

**Environment Variable**:
```
ConnectionStrings__DefaultConnection = ${{Postgres.DATABASE_URL}}
```

### B. API Service

**Settings**:
- Service Name: `api`
- Root Directory: `CalendarApp.API`

**Environment Variables** (copy these into Railway):
```
ConnectionStrings__DefaultConnection = ${{Postgres.DATABASE_URL}}

RabbitMQ__Host = [FILL FROM STEP 2]
RabbitMQ__Port = 5672
RabbitMQ__Username = [FILL FROM STEP 2]
RabbitMQ__Password = [FILL FROM STEP 2]

Jwt__SecretKey = SuperSecretKey_Railway_Production_2026_CalendarApp_SecureKey123456
Jwt__Issuer = CalendarApp.API
Jwt__Audience = CalendarApp.Client
Jwt__ExpiryMinutes = 15
Jwt__RefreshTokenExpiryDays = 7

Email__SmtpHost = smtp.gmail.com
Email__SmtpPort = 587
Email__SmtpUsername = mrpolermen@gmail.com
Email__SmtpPassword = klspuqjoaquhywlc
Email__FromEmail = mrpolermen@gmail.com
Email__FromName = Calendar App

ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://0.0.0.0:$PORT
```

**API URL** (after deployment):
```
https://________________________________
```

### C. Consumer Service

**Settings**:
- Service Name: `consumer`
- Root Directory: `CalendarApp.MessageConsumer`

**Environment Variables**:
```
RabbitMQ__Host = [FILL FROM STEP 2]
RabbitMQ__Port = 5672
RabbitMQ__Username = [FILL FROM STEP 2]
RabbitMQ__Password = [FILL FROM STEP 2]

Email__SmtpHost = smtp.gmail.com
Email__SmtpPort = 587
Email__SmtpUsername = mrpolermen@gmail.com
Email__SmtpPassword = klspuqjoaquhywlc
Email__FromEmail = mrpolermen@gmail.com
Email__FromName = Calendar App
```

### D. Frontend Service

**Settings**:
- Service Name: `frontend`
- Root Directory: `calendar-frontend`

**Environment Variables** (use API URL from B):
```
VITE_API_BASE_URL = https://[YOUR-API-URL]/api
```

**Frontend URL** (after deployment):
```
https://________________________________
```

### E. Update API CORS

Go back to API service → Variables → Add:
```
AllowedOrigins__0 = https://[YOUR-FRONTEND-URL]
```

---

## Step 4: Testing Checklist

- [ ] Frontend loads at URL
- [ ] Register with: mrpolermen@gmail.com
- [ ] Check email for welcome message
- [ ] Login successfully
- [ ] Create a task
- [ ] Share calendar with another email
- [ ] That email receives notification

---

## Quick Reference

**Your Accounts**:
- GitHub: polermen
- Email: mrpolermen@gmail.com
- Railway: (login with GitHub)
- CloudAMQP: (login with GitHub)

**Your Repository**:
- https://github.com/polermen/calendar-app

**Deployment Guides**:
- [QUICK_DEPLOY.md](QUICK_DEPLOY.md) - Quick version
- [DEPLOY_NOW.md](DEPLOY_NOW.md) - Detailed step-by-step
- [RAILWAY_DEPLOYMENT.md](RAILWAY_DEPLOYMENT.md) - Complete Railway guide

---

## Next Action

**Go to CloudAMQP**: https://www.cloudamqp.com

1. Sign up with GitHub
2. Create free instance
3. Fill in CloudAMQP section above
4. Then proceed to Railway deployment!

---

**Estimated time remaining**: 40 minutes
