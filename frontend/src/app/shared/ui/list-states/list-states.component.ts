import { Component, input } from '@angular/core';

/** Skeleton + empty + error states shared by every list page. */
@Component({
  selector: 'app-loading-state',
  standalone: true,
  template: `
    <div class="space-y-4" aria-label="Loading" role="status">
      @for (row of [1, 2, 3]; track row) {
        <div class="card flex gap-4 p-5">
          <div class="skeleton hidden h-28 w-40 shrink-0 sm:block"></div>
          <div class="min-w-0 flex-1 space-y-3">
            <div class="skeleton h-5 w-1/3"></div>
            <div class="skeleton h-4 w-full"></div>
            <div class="skeleton h-4 w-5/6"></div>
            <div class="flex gap-2 pt-1">
              <div class="skeleton h-5 w-14 rounded-full"></div>
              <div class="skeleton h-5 w-16 rounded-full"></div>
              <div class="skeleton h-5 w-12 rounded-full"></div>
            </div>
          </div>
        </div>
      }
    </div>
  `,
})
export class LoadingStateComponent {}

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="card border-dashed px-6 py-14 text-center">
      <svg
        class="mx-auto mb-4 h-10 w-10 text-amber-500/40"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        stroke-width="1.5"
        aria-hidden="true"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M20.25 7.5l-.625 10.632a2.25 2.25 0 01-2.247 2.118H6.622a2.25 2.25 0 01-2.247-2.118L3.75 7.5m8.25 3v6.75m0 0l-3-3m3 3l3-3M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125z"
        />
      </svg>
      <p class="text-neutral-400">Nothing here yet — check back soon.</p>
    </div>
  `,
})
export class EmptyStateComponent {}

@Component({
  selector: 'app-error-state',
  standalone: true,
  template: `
    <div class="card border-red-900/70 bg-red-950/40 px-6 py-12 text-center">
      <svg
        class="mx-auto mb-4 h-9 w-9 text-red-400/60"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        stroke-width="1.5"
        aria-hidden="true"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"
        />
      </svg>
      <p class="font-medium text-red-200">Something went wrong loading this content.</p>
      <p class="mt-1.5 text-sm text-red-300/70">Please try again in a moment.</p>
      @if (retryable()) {
        <button type="button" class="btn btn-outline mt-5 border-red-800 text-red-200" (click)="onRetry()">
          Try again
        </button>
      }
    </div>
  `,
})
export class ErrorStateComponent {
  /** Enables the retry button when the caller supplies (reload) behavior. */
  readonly retryable = input(false);
  readonly retry = input<() => void>();

  onRetry(): void {
    this.retry()?.();
  }
}
