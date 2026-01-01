# Deployment Checklist

Use this checklist to ensure you've completed all necessary steps for deploying your Calendar App to Railway.

## Pre-Deployment

### Code Repository
- [ ] All code committed to Git
- [ ] Repository pushed to GitHub
- [ ] `.env` files excluded from Git (should be in `.gitignore`)
- [ ] No sensitive credentials hardcoded in source files

### Configuration Files
- [ ] `appsettings.Production.json` created for API
- [ ] `appsettings.Production.json` created for MessageConsumer
- [ ] Dockerfiles created for all services (API, Consumer, Frontend)
- [ ] `docker-compose.yml` created for local testing
- [ ] `.dockerignore` files created

### Local Testing
- [ ] Project builds successfully: `dotnet build`
- [ ] All tests pass (if any)
- [ ] Docker Compose runs successfully: `docker-compose up`
- [ ] Can register a new user locally
- [ ] Welcome email received after registration
- [ ] Can share calendar with another user
- [ ] Shared calendar notification email received
- [ ] RabbitMQ queues working (check at http://localhost:15672)

## Railway Setup

### Account and Project
- [ ] Railway account created
- [ ] New Railway project created
- [ ] Repository connected to Railway

### Database Setup
- [ ] PostgreSQL database added to Railway project
- [ ] Database connection string noted
- [ ] Npgsql.EntityFrameworkCore.PostgreSQL package installed in API

### RabbitMQ Setup (CloudAMQP)
- [ ] CloudAMQP account created
- [ ] Free tier instance created
- [ ] AMQP URL copied
- [ ] Host, username, and password extracted from URL

### Email Configuration
- [ ] Gmail account prepared for sending emails
- [ ] 2-Factor Authentication enabled on Gmail
- [ ] Gmail App Password generated
- [ ] App Password saved securely

## Railway Services Deployment

### API Service
- [ ] API service created in Railway
- [ ] Root directory set to `CalendarApp.API`
- [ ] Dockerfile path configured
- [ ] Environment variables configured:
  - [ ] `ConnectionStrings__DefaultConnection` (from PostgreSQL)
  - [ ] `RabbitMQ__Host`
  - [ ] `RabbitMQ__Port`
  - [ ] `RabbitMQ__Username`
  - [ ] `RabbitMQ__Password`
  - [ ] `Jwt__SecretKey` (strong, unique value)
  - [ ] `Jwt__Issuer`
  - [ ] `Jwt__Audience`
  - [ ] `Jwt__ExpiryMinutes`
  - [ ] `Jwt__RefreshTokenExpiryDays`
  - [ ] `Email__SmtpHost`
  - [ ] `Email__SmtpPort`
  - [ ] `Email__SmtpUsername`
  - [ ] `Email__SmtpPassword`
  - [ ] `Email__FromEmail`
  - [ ] `Email__FromName`
  - [ ] `ASPNETCORE_ENVIRONMENT=Production`
  - [ ] `ASPNETCORE_URLS=http://0.0.0.0:$PORT`
  - [ ] `AllowedOrigins__0` (frontend URL)
- [ ] Service deployed successfully
- [ ] Public domain generated/noted
- [ ] Health check passing

### Message Consumer Service
- [ ] Consumer service created in Railway
- [ ] Root directory set to `CalendarApp.MessageConsumer`
- [ ] Dockerfile path configured
- [ ] Environment variables configured:
  - [ ] `RabbitMQ__Host`
  - [ ] `RabbitMQ__Port`
  - [ ] `RabbitMQ__Username`
  - [ ] `RabbitMQ__Password`
  - [ ] `Email__SmtpHost`
  - [ ] `Email__SmtpPort`
  - [ ] `Email__SmtpUsername`
  - [ ] `Email__SmtpPassword`
  - [ ] `Email__FromEmail`
  - [ ] `Email__FromName`
- [ ] Service deployed successfully
- [ ] No errors in logs

### Frontend Service
- [ ] Frontend service created in Railway
- [ ] Root directory set to `calendar-frontend`
- [ ] Build configuration set (Nixpacks or Dockerfile)
- [ ] Environment variables configured:
  - [ ] `VITE_API_BASE_URL` (API service URL)
- [ ] Service deployed successfully
- [ ] Public domain generated/noted
- [ ] Can access frontend in browser

## Post-Deployment Configuration

### CORS Update
- [ ] API `AllowedOrigins` environment variable updated with frontend URL
- [ ] API service redeployed with new CORS settings

### Database Migrations
- [ ] Migrations run automatically on API startup (check logs)
- [ ] Database tables created successfully
- [ ] No migration errors in logs

## Testing in Production

### User Registration
- [ ] Can register new user from production frontend
- [ ] User data saved to PostgreSQL
- [ ] Welcome email sent successfully
- [ ] Welcome email received in inbox
- [ ] No errors in API logs
- [ ] No errors in Consumer logs
- [ ] Message appeared in RabbitMQ queue (check CloudAMQP dashboard)
- [ ] Message consumed successfully

### User Login
- [ ] Can login with registered user
- [ ] JWT token received
- [ ] Token stored in browser
- [ ] Authenticated requests work

### Calendar Sharing
- [ ] Can share calendar with another user's email
- [ ] Share saved to database
- [ ] Calendar shared notification email sent
- [ ] Notification email received
- [ ] Recipient can see shared calendar in "Spectate" view
- [ ] Shared calendar data displays correctly

### Task/Todo Management
- [ ] Can create tasks
- [ ] Can create todos
- [ ] Tasks/todos saved to database
- [ ] Can view tasks/todos
- [ ] Can update tasks/todos
- [ ] Can delete tasks/todos

## Monitoring and Logs

### API Service
- [ ] API logs reviewed for errors
- [ ] HTTP requests logging properly
- [ ] Database queries successful
- [ ] RabbitMQ message publishing working

### Consumer Service
- [ ] Consumer logs reviewed for errors
- [ ] RabbitMQ connection established
- [ ] Messages being consumed
- [ ] Emails being sent

### RabbitMQ (CloudAMQP)
- [ ] Connected to CloudAMQP dashboard
- [ ] Queues created: `user-registered`, `calendar-shared`
- [ ] Messages being published to queues
- [ ] Messages being consumed from queues
- [ ] No messages stuck in queues

### Frontend
- [ ] Frontend loads without errors
- [ ] Console has no errors
- [ ] API requests successful (check Network tab)
- [ ] All features working

## Security Review

- [ ] JWT secret key is strong and unique (not the example value)
- [ ] Database credentials not exposed in frontend
- [ ] SMTP credentials not exposed in frontend
- [ ] CORS only allows specific frontend domain
- [ ] HTTPS enforced (automatic with Railway)
- [ ] No sensitive data in Git repository
- [ ] No hardcoded credentials in code

## Performance and Reliability

- [ ] API responds quickly (< 500ms for most requests)
- [ ] Frontend loads quickly (< 3 seconds)
- [ ] Email delivery time acceptable (< 1 minute)
- [ ] Services restart on failure (Railway auto-restart configured)
- [ ] Database connection pooling working
- [ ] No memory leaks (check Railway metrics)

## Documentation

- [ ] README.md updated with deployment info
- [ ] Environment variables documented
- [ ] Deployment process documented
- [ ] Known issues documented

## Optional Enhancements

- [ ] Custom domain configured (instead of Railway subdomain)
- [ ] SSL certificate configured (automatic with custom domain)
- [ ] Monitoring/alerting set up (Railway webhooks, external services)
- [ ] Backup strategy for database
- [ ] CI/CD pipeline configured (GitHub Actions)
- [ ] Error tracking service integrated (Sentry, etc.)

## Rollback Plan

- [ ] Know how to rollback to previous deployment in Railway
- [ ] Database backup/restore procedure understood
- [ ] Emergency contact information documented

---

## Quick Reference

### Railway URLs
- **API**: https://calendarapp-api-production.up.railway.app
- **Frontend**: https://calendarapp-frontend-production.up.railway.app
- **PostgreSQL**: Internal (accessible only from Railway services)

### External Services
- **CloudAMQP Dashboard**: https://customer.cloudamqp.com/instance
- **Gmail Account**: https://mail.google.com

### Key Commands

#### View Railway Logs
```bash
railway logs --service api
railway logs --service consumer
railway logs --service frontend
```

#### Redeploy Service
```bash
railway up --service api
railway up --service consumer
railway up --service frontend
```

#### Check Service Status
```bash
railway status
```

---

## Support Resources

- Railway Docs: https://docs.railway.app
- CloudAMQP Docs: https://www.cloudamqp.com/docs
- Railway Discord: https://discord.gg/railway
- PostgreSQL Docs: https://www.postgresql.org/docs

---

**Last Updated**: _Fill in deployment date_

**Deployed By**: _Your name_

**Notes**: _Add any specific notes about your deployment_
