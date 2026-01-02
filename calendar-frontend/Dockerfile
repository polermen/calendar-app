# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy all source files
COPY . .

# Build the application
RUN npm run build

# Runtime stage
FROM node:20-alpine AS runtime
WORKDIR /app

# Install serve to run the built application
RUN npm install -g serve

# Copy built files from build stage
COPY --from=build /app/dist ./dist

# Expose port (Railway will assign dynamically)
EXPOSE 3000

# Serve the application on Railway's dynamic PORT
CMD sh -c "serve -s dist -p ${PORT:-3000}"
