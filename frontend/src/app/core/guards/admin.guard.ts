import { inject } from '@angular/core';
import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AdminAuthService } from '@core/services/admin-auth.service';

/**
 * Protects /admin/* (except the login page, which sits outside the guarded
 * route). On the server there is no localStorage, so it lets the request
 * through; the client-side navigation re-runs this guard with the real token
 * and redirects to /admin/login when missing/expired.
 */
export const adminGuard: CanActivateFn = (_route, state) => {
  const platformId = inject(PLATFORM_ID);
  if (!isPlatformBrowser(platformId)) {
    return true;
  }

  const auth = inject(AdminAuthService);
  if (auth.isAuthenticated()) {
    return true;
  }

  const router = inject(Router);
  return router.createUrlTree(['/admin/login'], {
    queryParams: { returnUrl: state.url },
  });
};
