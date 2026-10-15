import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminLoginResponse } from '@shared/models/admin.models';

const TOKEN_KEY = 'portfolio.admin.token';
const EXPIRY_KEY = 'portfolio.admin.expiresAtUtc';

/**
 * Admin JWT store. The token lives in memory (authoritative) and is mirrored
 * to localStorage so a page refresh keeps the session; the interceptor picks
 * it up for /admin/* API calls. SSR never touches localStorage.
 */
@Injectable({ providedIn: 'root' })
export class AdminAuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly platformId = inject(PLATFORM_ID);

  private readonly _token = signal<string | null>(null);
  private readonly _expiresAtUtc = signal<string | null>(null);

  readonly token = this._token.asReadonly();
  readonly expiresAtUtc = this._expiresAtUtc.asReadonly();

  readonly isAuthenticated = computed(() => {
    const expiry = this._expiresAtUtc();
    if (!this._token() || !expiry) {
      return false;
    }
    return new Date(expiry).getTime() > Date.now();
  });

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem(TOKEN_KEY);
      const expiry = localStorage.getItem(EXPIRY_KEY);
      if (token && expiry) {
        this._token.set(token);
        this._expiresAtUtc.set(expiry);
      }
    }
  }

  login(email: string, password: string): Observable<AdminLoginResponse> {
    return this.http
      .post<AdminLoginResponse>(`${environment.apiBaseUrl}/api/v1/admin/auth/login`, {
        email,
        password,
      })
      .pipe(
        tap((response) => {
          this._token.set(response.token);
          this._expiresAtUtc.set(response.expiresAtUtc);
          if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem(TOKEN_KEY, response.token);
            localStorage.setItem(EXPIRY_KEY, response.expiresAtUtc);
          }
        }),
      );
  }

  logout(): void {
    this._token.set(null);
    this._expiresAtUtc.set(null);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(EXPIRY_KEY);
      void this.router.navigateByUrl('/admin/login');
    }
  }

  /** Navigate to login remembering where the user was headed. */
  redirectToLogin(returnUrl: string): void {
    void this.router.navigate(['/admin/login'], { queryParams: { returnUrl } });
  }
}
