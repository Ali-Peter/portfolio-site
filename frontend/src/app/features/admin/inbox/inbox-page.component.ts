import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { AdminApiService } from '@core/services/admin-api.service';
import { AdminAuthService } from '@core/services/admin-auth.service';
import { ToastService } from '@core/services/toast.service';
import { AdminContactMessageDto } from '@shared/models/admin.models';

type HandledFilter = 'all' | 'unhandled' | 'handled';

/** Paged contact inbox with handled toggling and deletion. */
@Component({
  selector: 'app-admin-inbox-page',
  imports: [DatePipe],
  templateUrl: './inbox-page.component.html',
})
export class AdminInboxPageComponent implements OnInit {
  private readonly api = inject(AdminApiService);
  private readonly auth = inject(AdminAuthService);
  private readonly toast = inject(ToastService);

  protected readonly pageSize = 20;
  protected readonly items = signal<AdminContactMessageDto[]>([]);
  protected readonly page = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly totalPages = signal(1);
  protected readonly loading = signal(true);
  protected readonly filter = signal<HandledFilter>('all');

  ngOnInit(): void {
    this.reload();
  }

  protected reload(): void {
    if (!this.auth.isAuthenticated()) {
      this.loading.set(false);
      return;
    }
    const filter = this.filter();
    this.loading.set(true);
    this.api
      .getInbox(
        this.page(),
        this.pageSize,
        filter === 'all' ? undefined : filter === 'handled',
      )
      .subscribe({
        next: (result) => {
          this.items.set(result.items);
          this.page.set(result.page);
          this.totalCount.set(result.totalCount);
          this.totalPages.set(Math.max(result.totalPages, 1));
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  protected setFilter(filter: HandledFilter): void {
    this.filter.set(filter);
    this.page.set(1);
    this.reload();
  }

  protected goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }
    this.page.set(page);
    this.reload();
  }

  protected toggleHandled(message: AdminContactMessageDto): void {
    this.api.markHandled(message.id, !message.isHandled).subscribe({
      next: () => {
        this.items.update((items) =>
          items.map((item) =>
            item.id === message.id ? { ...item, isHandled: !message.isHandled } : item,
          ),
        );
        this.toast.success(message.isHandled ? 'Marked as unhandled.' : 'Marked as handled.');
      },
      error: () => undefined,
    });
  }

  protected remove(message: AdminContactMessageDto): void {
    if (!confirm(`Delete message from ${message.name}?`)) {
      return;
    }
    this.api.deleteMessage(message.id).subscribe({
      next: () => {
        this.toast.success('Message deleted.');
        this.reload();
      },
      error: () => undefined,
    });
  }

  protected trackById(_index: number, message: AdminContactMessageDto): string {
    return message.id;
  }
}
