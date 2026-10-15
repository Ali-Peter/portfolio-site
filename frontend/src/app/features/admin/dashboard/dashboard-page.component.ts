import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AdminApiService } from '@core/services/admin-api.service';
import { AdminAuthService } from '@core/services/admin-auth.service';
import { ToastService } from '@core/services/toast.service';

interface Stat {
  readonly label: string;
  readonly value: number | null;
  readonly href: string;
}

/** Quick counts across the CMS + manual search reindex trigger. */
@Component({
  selector: 'app-admin-dashboard-page',
  imports: [RouterLink],
  templateUrl: './dashboard-page.component.html',
})
export class AdminDashboardPageComponent implements OnInit {
  private readonly api = inject(AdminApiService);
  private readonly auth = inject(AdminAuthService);
  private readonly toast = inject(ToastService);

  protected readonly loading = signal(true);
  protected readonly projectCount = signal<number | null>(null);
  protected readonly postCount = signal<number | null>(null);
  protected readonly inboxCount = signal<number | null>(null);

  protected readonly reindexing = signal(false);
  protected readonly lastReindexed = signal<string | null>(null);

  protected readonly stats: Stat[] = [
    { label: 'Projects', value: 0, href: '/admin/projects' },
    { label: 'Blog posts', value: 0, href: '/admin/blog' },
    { label: 'Inbox messages', value: 0, href: '/admin/inbox' },
  ];

  ngOnInit(): void {
    // Server-side rendering has no token; the client re-runs this with one.
    if (!this.auth.isAuthenticated()) {
      this.loading.set(false);
      return;
    }
    forkJoin({
      projects: this.api.getProjects(),
      posts: this.api.getPosts(),
      inbox: this.api.getInbox(1, 1),
    }).subscribe({
      next: ({ projects, posts, inbox }) => {
        this.projectCount.set(projects.length);
        this.postCount.set(posts.length);
        this.inboxCount.set(inbox.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected reindex(): void {
    if (this.reindexing()) {
      return;
    }
    this.reindexing.set(true);
    this.api.reindex().subscribe({
      next: ({ chunks }) => {
        this.reindexing.set(false);
        this.lastReindexed.set(new Date().toLocaleTimeString());
        this.toast.success(`Search index rebuilt — ${chunks} chunks.`);
      },
      error: () => this.reindexing.set(false),
    });
  }
}
