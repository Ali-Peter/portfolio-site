import { Component, inject } from '@angular/core';
import { ToastService } from '@core/services/toast.service';

/**
 * Fixed toast host rendered by the site layout. Purely presentational:
 * all state lives in {@link ToastService}.
 */
@Component({
  selector: 'app-toast-host',
  standalone: true,
  template: `
    <div class="pointer-events-none fixed right-4 top-4 z-50 flex w-80 flex-col gap-2" role="status">
      @for (toast of toasts(); track toast.id) {
        <div
          class="pointer-events-auto flex items-start justify-between gap-2 rounded-md border px-3 py-2 text-sm shadow-lg"
          [class]="toastClass(toast.kind)"
        >
          <span>{{ toast.message }}</span>
          <button
            type="button"
            (click)="dismiss(toast.id)"
            class="ml-2 shrink-0 text-neutral-400 hover:text-white"
            aria-label="Dismiss notification"
          >
            ✕
          </button>
        </div>
      }
    </div>
  `,
})
export class ToastHostComponent {
  private readonly toastService = inject(ToastService);

  protected readonly toasts = this.toastService.toasts;

  protected dismiss(id: number): void {
    this.toastService.dismiss(id);
  }

  protected toastClass(kind: 'success' | 'error' | 'info'): string {
    switch (kind) {
      case 'success':
        return 'border-emerald-700 bg-emerald-950 text-emerald-100';
      case 'error':
        return 'border-red-800 bg-red-950 text-red-100';
      default:
        return 'border-neutral-700 bg-neutral-900 text-neutral-100';
    }
  }
}
