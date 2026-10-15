import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { AdminAuthService } from '@core/services/admin-auth.service';

/**
 * Attaches the admin JWT to admin API calls (URLs containing /api/v1/admin/).
 * A 401 on that surface means the token expired or was revoked: drop it and
 * send the user back to the login page. Public endpoints never match, so
 * browsing the site stays tokenless.
 */
export function authInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  if (!req.url.includes('/api/v1/admin/')) {
    return next(req);
  }

  const auth = inject(AdminAuthService);
  const token = auth.token();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        auth.logout();
      }
      return throwError(() => error);
    }),
  );
}
