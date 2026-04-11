# # Multi-stage build for Next.js web app
# # Stage 1: Dependencies
# FROM node:20-alpine AS deps
# WORKDIR /app
# COPY package.json package-lock.json ./
# RUN npm ci

# # Stage 2: Builder
# FROM node:20-alpine AS builder
# WORKDIR /app
# COPY --from=deps /app/node_modules ./node_modules
# COPY . .
# RUN npm run build

# # Stage 3: Runner
# FROM node:20-alpine AS runner
# WORKDIR /app
# ENV NODE_ENV=production
# COPY --from=builder /app/.next ./.next
# COPY --from=builder /app/public ./public
# COPY --from=builder /app/package.json ./
# COPY --from=builder /app/node_modules ./node_modules
# EXPOSE 3000
# CMD ["npm", "start"]

# ----------------------
# Stage 1: Install deps
# ----------------------
FROM node:20-alpine AS deps
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

# ----------------------
# Stage 2: Build app
# ----------------------
FROM node:20-alpine AS builder
WORKDIR /app

COPY --from=deps /app/node_modules ./node_modules
COPY . .

RUN npm run build

# ----------------------
# Stage 3: Production runner
# ----------------------
FROM node:20-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production

# ONLY install production deps
COPY package.json package-lock.json ./
RUN npm ci --omit=dev

# Copy only required build output
COPY --from=builder /app/.next ./.next
COPY --from=builder /app/public ./public

EXPOSE 3000

CMD ["npm", "start"]