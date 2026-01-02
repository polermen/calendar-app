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

# Copy built files from build stage
COPY --from=build /app/dist ./dist

# Copy server.js and package files
COPY server.js .
COPY package*.json .

# Install only production dependencies (express)
RUN npm install --production

# Expose port (Railway will assign dynamically)
EXPOSE 3000

# Start the Node.js server
CMD ["node", "server.js"]
