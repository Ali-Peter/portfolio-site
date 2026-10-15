import { PLATFORM_ID, WritableSignal, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Observable } from 'rxjs';

/**
 * Shared data-loading helper for public pages.
 *
 * During SSR/prerender there is no API to call (and no point calling it —
 * prerendered shells hydrate and fetch client-side), so server renders
 * short-circuit to the loaded state and only the browser subscribes.
 * This keeps every page prerender-safe without coupling builds to a live API.
 */
export abstract class ServerSafePage {
  private readonly platformId = inject(PLATFORM_ID);

  protected readonly isBrowser = isPlatformBrowser(this.platformId);

  protected loadOnBrowser<T>(
    loading: WritableSignal<boolean>,
    error: WritableSignal<boolean>,
    source: Observable<T>,
    onValue: (value: T) => void,
  ): void {
    if (!this.isBrowser) {
      loading.set(false);
      return;
    }
    source.subscribe({
      next: (value) => {
        onValue(value);
        loading.set(false);
      },
      error: () => {
        error.set(true);
        loading.set(false);
      },
    });
  }
}
