/**
 * Production environment configuration.
 *
 * apiBaseUrl MUST point at the live API host. The value below is a
 * placeholder — replace it at deploy time with one of:
 *   1. Edit this file before `ng build --configuration production`, or
 *   2. Set API_BASE_URL as a build-time env var (see frontend/Dockerfile),
 *      e.g. `docker build --build-arg API_BASE_URL=https://api.example.com .`
 */
export const environment = {
  production: true,
  apiBaseUrl: '__API_BASE_URL__',
};
