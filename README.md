# Calendar Web Application

A modern, full-featured calendar application with task management, todo lists, calendar sharing, and automated email notifications. Built with .NET 8 backend and React frontend, featuring asynchronous message processing via RabbitMQ.

## Features

### Core Features
- **User Authentication**: Secure JWT-based authentication with refresh tokens
- **Task Management**: Create, read, update, and delete tasks with due dates
- **Todo Lists**: Organize todos by day, week, month, or year
- **Calendar Sharing**: Share your calendar with other users (read-only access)
- **Email Notifications**: Automated emails for user registration and calendar sharing
- **Spectate Mode**: View calendars shared with you
- **Real-time Messaging**: Asynchronous processing with RabbitMQ

### Technical Highlights
- Message queue architecture for reliable email delivery
- Background worker service for non-blocking operations
- Dual database support (SQL Server local, PostgreSQL production)
- Production-ready deployment configuration for Railway
- Docker and Docker Compose support for local development
- RESTful API with Swagger documentation

## Tech Stack

### Backend
- **Framework**: .NET 8 / ASP.NET Core Web API
- **Database**: SQL Server (local) / PostgreSQL (production)
- **Cache**: Redis (optional)
- **Message Queue**: RabbitMQ
- **Authentication**: JWT Bearer tokens with refresh tokens
- **ORM**: Entity Framework Core
- **Email**: SMTP (Gmail compatible)

### Frontend
- **Framework**: React 18 with Vite
- **HTTP Client**: Axios with interceptors
- **Routing**: React Router
- **Styling**: CSS3 with custom components

### DevOps
- **Containerization**: Docker & Docker Compose
- **Cloud Platform**: Railway
- **Message Broker**: CloudAMQP (managed RabbitMQ)
- **CI/CD**: GitHub integration with auto-deploy

## Project Structure

```
ThisIsIt/
├── CalendarApp.API/              # ASP.NET Core Web API
│   ├── Controllers/              # API endpoints
│   ├── Data/                     # Database context and repositories
│   ├── Models/                   # Entities, DTOs, and message models
│   ├── Services/                 # Business logic services
│   ├── Migrations/               # EF Core migrations
│   ├── Dockerfile                # Production Docker image
│   └── appsettings.json          # Configuration
│
├── CalendarApp.MessageConsumer/  # Background Worker Service
│   ├── Workers/                  # RabbitMQ consumers
│   ├── Services/                 # Email service
│   ├── Models/                   # Message models
│   ├── Dockerfile                # Production Docker image
│   └── appsettings.json          # Configuration
│
├── calendar-frontend/            # React Frontend
│   ├── src/
│   │   ├── components/           # React components
│   │   ├── services/             # API and auth services
│   │   ├── pages/                # Page components
│   │   └── App.jsx               # Main app component
│   ├── Dockerfile                # Production Docker image
│   └── package.json              # Dependencies
│
├── docker-compose.yml            # Local development environment
├── .env.example                  # Environment variables template
├── DOCKER_SETUP.md               # Docker setup guide
├── RAILWAY_DEPLOYMENT.md         # Railway deployment guide
├── DEPLOYMENT_CHECKLIST.md       # Deployment checklist
└── README.md                     # This file
```

## Quick Start

### Option 1: Docker Compose (Recommended)

The fastest way to get started:

```bash
# 1. Clone the repository
git clone <your-repo-url>
cd ThisIsIt

# 2. Configure email settings
cp .env.example .env
# Edit .env and add your Gmail App Password

# 3. Start all services
docker-compose up --build

# 4. Run migrations
docker-compose exec api dotnet ef database update

# 5. Open your browser
# Frontend: http://localhost:5173
# API: http://localhost:5000
# RabbitMQ UI: http://localhost:15672 (guest/guest)
```

See [DOCKER_SETUP.md](DOCKER_SETUP.md) for detailed Docker instructions.

### Option 2: Manual Setup

#### Prerequisites
- .NET 8.0 SDK
- Node.js 20+
- SQL Server or PostgreSQL
- RabbitMQ

#### 1. Database Setup

Using SQL Server:
```bash
cd CalendarApp.API
dotnet ef database update
```

#### 2. Configure appsettings.json

Edit `CalendarApp.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CalendarAppDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": "5672",
    "Username": "guest",
    "Password": "guest"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-gmail-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Calendar App"
  }
}
```

#### 3. Start RabbitMQ

```bash
# Using Docker
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management-alpine
```

#### 4. Run the API

```bash
cd CalendarApp.API
dotnet run
```

#### 5. Run the Message Consumer

```bash
cd CalendarApp.MessageConsumer
dotnet run
```

#### 6. Run the Frontend

```bash
cd calendar-frontend
npm install
npm run dev
```

## Email Configuration (Gmail)

To send emails using Gmail:

1. Enable 2-Factor Authentication on your Google account
2. Generate an App Password:
   - Go to https://myaccount.google.com/apppasswords
   - Select "Mail" and "Other"
   - Copy the 16-character password
3. Use this App Password in your configuration (not your regular Gmail password)

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/refresh-token` - Refresh access token
- `POST /api/auth/logout` - Logout user (requires auth)
- `GET /api/auth/me` - Get current user (requires auth)

### Tasks (requires authentication)
- `GET /api/tasks` - Get all tasks for current user
- `GET /api/tasks/{id}` - Get specific task
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task

### Todos (requires authentication)
- `GET /api/todos` - Get all todos for current user
- `GET /api/todos/{id}` - Get specific todo
- `POST /api/todos` - Create new todo
- `PUT /api/todos/{id}` - Update todo
- `DELETE /api/todos/{id}` - Delete todo

### Calendar Sharing (requires authentication)
- `GET /api/share/my-shares` - Get calendars you've shared
- `GET /api/share/spectating` - Get calendars shared with you
- `POST /api/share` - Share your calendar with another user
- `DELETE /api/share/{id}` - Remove calendar share

### Shared Calendar Data (requires authentication)
- `GET /api/sharedcalendar/{ownerId}/tasks` - Get tasks from shared calendar
- `GET /api/sharedcalendar/{ownerId}/todos` - Get todos from shared calendar

## Message Queue Architecture

The application uses RabbitMQ for asynchronous email delivery:

```
User Action (Register/Share)
         ↓
    API Endpoint
         ↓
   Publish Message → RabbitMQ Queue
                          ↓
                    Message Consumer
                          ↓
                     Send Email
```

**Message Queues:**
- `user-registered`: Handles welcome emails when users register
- `calendar-shared`: Handles notification emails when calendars are shared

**Benefits:**
- API responses are fast (not blocked by email sending)
- Reliable message delivery
- Email failures don't break user experience
- Can scale consumers independently

## Deployment to Railway

This application is ready for deployment to Railway with full PostgreSQL and RabbitMQ support.

See the comprehensive [RAILWAY_DEPLOYMENT.md](RAILWAY_DEPLOYMENT.md) guide for:
- Step-by-step deployment instructions
- CloudAMQP (managed RabbitMQ) setup
- PostgreSQL database configuration
- Environment variable configuration
- Production checklist

Use the [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md) to ensure you've completed all necessary steps.

## Development

### Adding Database Migrations

```bash
cd CalendarApp.API
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### Viewing Logs

**Docker Compose:**
```bash
docker-compose logs -f api
docker-compose logs -f consumer
```

**Railway:**
```bash
railway logs --service api
railway logs --service consumer
```

### Monitoring RabbitMQ

**Local:**
- Visit http://localhost:15672
- Login: guest/guest
- View queues, messages, and consumers

**CloudAMQP (Production):**
- Login to CloudAMQP dashboard
- Go to RabbitMQ Manager
- Monitor queue depths and message rates

## Testing

### Manual Testing Flow

1. **User Registration**
   - Register a new user via frontend
   - Check email for welcome message
   - Verify user in database

2. **Authentication**
   - Login with credentials
   - Verify JWT token storage
   - Test protected endpoints

3. **Task Management**
   - Create tasks with various due dates
   - Update task details
   - Mark tasks complete
   - Delete tasks

4. **Calendar Sharing**
   - Share calendar with another user's email
   - Check recipient's email for notification
   - Login as recipient
   - View shared calendar in Spectate mode
   - Verify read-only access

## Security Considerations

- **JWT Tokens**: 15-minute access tokens, 7-day refresh tokens
- **Password Hashing**: BCrypt with salt
- **CORS**: Restricted to specific origins
- **HTTPS**: Enforced in production (automatic on Railway)
- **Environment Variables**: All sensitive data via env vars
- **SQL Injection**: Protected via EF Core parameterization
- **XSS Protection**: React's built-in escaping

### Production Security Checklist
- [ ] Change default JWT secret key
- [ ] Use strong, unique passwords
- [ ] Enable HTTPS only
- [ ] Configure production CORS origins
- [ ] Secure RabbitMQ credentials
- [ ] Use Gmail App Password (not regular password)
- [ ] Review all environment variables
- [ ] Enable database connection encryption

## Performance

- Async/await throughout for non-blocking I/O
- Database connection pooling enabled
- Message queue prevents blocking on email sends
- EF Core query optimization with includes
- Frontend code splitting and lazy loading
- Efficient database indexes on foreign keys

## Troubleshooting

### API Won't Start
- Check database connection string
- Verify database is running
- Check port 5000 availability
- Review startup logs

### Emails Not Sending
- Verify Gmail App Password is correct
- Check Consumer service is running
- View Consumer logs for errors
- Check RabbitMQ dashboard for messages
- Verify SMTP settings

### Frontend Can't Connect
- Verify `VITE_API_BASE_URL` is correct
- Check API is running
- Review CORS configuration
- Check browser console for errors

### RabbitMQ Connection Issues
- Ensure RabbitMQ is running
- Verify host, port, credentials
- Check firewall settings
- Review service logs

## Future Enhancements

- [ ] Push notifications for calendar updates
- [ ] Recurring tasks and events
- [ ] Calendar import/export (iCal format)
- [ ] Mobile application (React Native)
- [ ] Real-time updates (SignalR)
- [ ] Advanced sharing permissions
- [ ] Calendar categories and tags
- [ ] Search and filtering
- [ ] User profile customization
- [ ] Dark mode
- [ ] Timezone support
- [ ] Email digest notifications

## Documentation

- [DOCKER_SETUP.md](DOCKER_SETUP.md) - Docker and Docker Compose guide
- [RAILWAY_DEPLOYMENT.md](RAILWAY_DEPLOYMENT.md) - Railway deployment guide
- [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md) - Pre-deployment checklist
- [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) - Detailed architecture documentation

## License

This project is for educational/demonstration purposes.

## Support

For issues, questions, or contributions:
- Review the documentation files listed above
- Check troubleshooting sections
- Review API logs and error messages
- Test with Docker Compose locally first
- Consult Railway docs for deployment issues

---

**Built with .NET 8, React, and RabbitMQ**
