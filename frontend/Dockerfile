# Angular 20 SSR — multi-stage production image.
# Build context: frontend/ directory.
#   docker build -f Dockerfile --build-arg API_BASE_URL=https://api.example.com -t portfolio-web ./frontend
# Run (production):
#   docker run -p 4000:4000 -e PORT=4000 portfolio-web
FROM node:22-alpine AS build
WORKDIR /app

# Production base URL for the .NET API (baked into environment.prod.ts at build
# time via sed below). Fails closed with the __API_BASE_URL__ placeholder so a
# missing arg is obvious instead of silently pointing at localhost.
ARG API_BASE_URL=__API_BASE_URL__
ENV API_BASE_URL=${API_BASE_URL}

COPY package.json package-lock.json ./
# npm ci = reproducible production install from the lockfile.
RUN npm ci

COPY . ./
# Swap the placeholder with the real API URL before the production build.
# (Angular file-replacement keeps environment.prod.ts as the prod source, and
# sed avoids adding a code-generation step to the repo.)
RUN if [ "$API_BASE_URL" = "__API_BASE_URL__" ]; then \
      echo "WARNING: building with placeholder API URL (__API_BASE_URL__). Pass --build-arg API_BASE_URL=https://api.example.com"; \
    else \
      sed -i "s#__API_BASE_URL__#${API_BASE_URL}#g" src/environments/environment.prod.ts; \
    fi \
    && npm run build

FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production \
    PORT=4000

# Only the SSR server output + its runtime deps (no devDependencies, no source).
COPY --from=build /app/dist ./dist
COPY --from=build /app/package.json /app/package-lock.json ./
RUN npm ci --omit=dev && npm cache clean --force \
    && apk add --no-cache wget

USER node
EXPOSE 4000

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget -qO- http://localhost:4000/ || exit 1

CMD ["node", "dist/frontend/server/server.mjs"]
