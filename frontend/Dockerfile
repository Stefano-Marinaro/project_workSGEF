# --- Stage 1: build della versione web dell'app Expo ---
FROM node:22-alpine AS build
WORKDIR /app

# Prima solo i file delle dipendenze: Docker riusa la cache di questo layer
# finché package.json / package-lock.json non cambiano.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# URL del backend visto dal BROWSER (non dal container): viene incollato nel bundle a build-time.
ARG EXPO_PUBLIC_API_URL=http://localhost:5161
ENV EXPO_PUBLIC_API_URL=$EXPO_PUBLIC_API_URL

RUN npx expo export --platform web --output-dir dist

# --- Stage 2: server web leggero che serve i file statici ---
FROM nginx:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
