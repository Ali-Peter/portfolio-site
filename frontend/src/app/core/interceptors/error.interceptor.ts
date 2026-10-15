import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ToastService } from '@core/services/toast.service';
import { ProblemDetails } from '@core/models/problem-details';

/**
 * Global HTTP error mapping. 422 responses carry per-field `errors` and are
 * left to the originating form (they are *not* toasted); everything else
 * becomes a friendly toast. The original error is always rethrown so callers
 * can still branch on status codes (404 pages, etc.).
 */
export function errorInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        // Admin 401s are silent: expired sessions redirect via authInterceptor
        // and the login form renders its own invalid-credentials message.
        const isAdminAuthError =
          error.status === 401 && req.url.includes('/api/v1/admin/');
        if (error.status !== 422 && !isAdminAuthError && !hasFieldErrors(error.error)) {
          toast.error(friendlyMessage(error));
        }
      }
      return throwError(() => error);
    }),
  );
}

function hasFieldErrors(body: unknown): body is ProblemDetails {
  return (
    typeof body === 'object' &&
    body !== null &&
    'errors' in body &&
    typeof (body as ProblemDetails).errors === 'object'
  );
}

function friendlyMessage(error: HttpErrorResponse): string {
  const problem = error.error as ProblemDetails | undefined;

  switch (error.status) {
    case 0:
      return 'The API is unreachable. Please check your connection and try again.';
    case 404:
      return problem?.detail ?? 'The requested item was not found.';
    case 422:
      return problem?.title ?? 'Some fields need attention.';
    case 429:
      return 'Too many requests — please wait a moment and try again.';
    default:
      if (error.status >= 500) {
        return 'Something went wrong on our side. Please try again later.';
      }
      return problem?.detail ?? problem?.title ?? 'Something went wrong. Please try again.';
  }
}
