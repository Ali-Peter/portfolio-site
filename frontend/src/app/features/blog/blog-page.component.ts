import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PortfolioApiService } from '@core/services/portfolio-api.service';
import { ServerSafePage } from '@shared/server-safe-page';
import { PageHeaderComponent } from '@shared/ui/page-header/page-header.component';
import {
  EmptyStateComponent,
  ErrorStateComponent,
  LoadingStateComponent,
} from '@shared/ui/list-states/list-states.component';
import { BlogPostDto } from '@shared/models/api.models';

/** Published posts, newest first, with substring search. */
@Component({
  selector: 'app-blog-page',
  standalone: true,
  imports: [PageHeaderComponent, RouterLink, LoadingStateComponent, EmptyStateComponent, ErrorStateComponent],
  template: `
    <app-page-header
      kicker="Writing"
      title="Blog"
      subtitle="Notes on .NET, Angular, PostgreSQL and applied AI."
    />
    <div class="relative mb-9 max-w-md">
      <svg
        class="pointer-events-none absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-neutral-500"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        stroke-width="2"
        aria-hidden="true"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M21 21l-4.35-4.35M17 10.5a6.5 6.5 0 11-13 0 6.5 6.5 0 0113 0z"
        />
      </svg>
      <input
        id="blog-search"
        type="search"
        [value]="search()"
        (input)="onSearchInput($any($event.target).value)"
        placeholder="Search articles…"
        class="input pl-10"
        aria-label="Search articles"
      />
    </div>

    @if (loading()) {
      <app-loading-state />
    } @else if (error()) {
      <app-error-state />
    } @else if (posts().length === 0) {
      <app-empty-state />
    } @else {
      <div class="grid gap-5">
        @for (post of posts(); track post.slug) {
          <a
            [routerLink]="['/blog', post.slug]"
            class="card card-hover group flex flex-col gap-3 p-6 sm:flex-row sm:items-center sm:justify-between"
          >
            <div class="min-w-0">
              <div class="flex flex-wrap items-center gap-2">
                @for (tag of post.tags; track tag) {
                  <span class="chip chip-accent">{{ tag }}</span>
                }
              </div>
              <h2 class="mt-2 text-xl font-semibold text-white transition group-hover:text-amber-300">
                {{ post.title }}
              </h2>
              <p class="mt-1.5 line-clamp-2 text-sm leading-relaxed text-neutral-400">
                {{ post.excerpt }}
              </p>
              <p class="mt-2.5 text-xs text-neutral-500">{{ meta(post) }}</p>
            </div>
            <span
              class="shrink-0 text-sm text-amber-400 transition group-hover:translate-x-1"
              aria-hidden="true"
            >
              →
            </span>
          </a>
        }
      </div>
    }
  `,
})
export class BlogPageComponent extends ServerSafePage implements OnInit {
  private readonly api = inject(PortfolioApiService);

  protected readonly posts = signal<readonly BlogPostDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal(false);
  protected readonly search = signal('');

  private debounce: ReturnType<typeof setTimeout> | undefined;

  ngOnInit(): void {
    this.load();
  }

  protected onSearchInput(value: string): void {
    this.search.set(value);
    clearTimeout(this.debounce);
    this.debounce = setTimeout(() => this.load(value.trim() || undefined), 300);
  }

  protected meta(post: BlogPostDto): string {
    const date = post.publishedAt ? new Date(post.publishedAt).toLocaleDateString() : 'Draft';
    return `${date} · ${post.readingMinutes} min read`;
  }

  private load(search?: string): void {
    this.loading.set(true);
    this.error.set(false);
    this.loadOnBrowser(this.loading, this.error, this.api.getPosts(1, 20, search), (page) => {
      this.posts.set(page.items);
    });
  }
}
