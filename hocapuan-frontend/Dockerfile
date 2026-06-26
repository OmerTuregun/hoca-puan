# ─── Dev Stage (docker-compose dev için) ────────────────────
FROM node:20-alpine AS dev
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
EXPOSE 5173
CMD ["npm", "run", "dev"]

# ─── Build Stage ─────────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app
ARG VITE_SENTRY_DSN=
ENV VITE_SENTRY_DSN=${VITE_SENTRY_DSN}
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# ─── Production Stage ────────────────────────────────────────
FROM nginx:alpine AS production
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
