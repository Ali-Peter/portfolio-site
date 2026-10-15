import { Injectable, signal } from '@angular/core';

export interface Toast {
  readonly id: number;
  readonly kind: 'success' | 'error' | 'info';
  readonly message: string;
}

/**
 * Minimal toast store. Components push; the layout's toast host renders.
 * Toasts auto-expire so a stuck error can never linger forever.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  private readonly _toasts = signal<readonly Toast[]>([]);

  readonly toasts = this._toasts.asReadonly();

  success(message: string): void {
    this.push('success', message);
  }

  error(message: string): void {
    this.push('error', message);
  }

  info(message: string): void {
    this.push('info', message);
  }

  dismiss(id: number): void {
    this._toasts.update((toasts) => toasts.filter((t) => t.id !== id));
  }

  private push(kind: Toast['kind'], message: string): void {
    const id = this.nextId++;
    this._toasts.update((toasts) => [...toasts, { id, kind, message }]);
    setTimeout(() => this.dismiss(id), 6000);
  }
}
