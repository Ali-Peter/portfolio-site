import { Component, OnInit, inject, signal } from '@angular/core';
import { PortfolioApiService } from '@core/services/portfolio-api.service';
import { ServerSafePage } from '@shared/server-safe-page';
import { PageHeaderComponent } from '@shared/ui/page-header/page-header.component';
import {
  EmptyStateComponent,
  ErrorStateComponent,
  LoadingStateComponent,
} from '@shared/ui/list-states/list-states.component';
import { ExperienceDto } from '@shared/models/api.models';

/** Career timeline — reverse-chronological entries straight from the API. */
@Component({
  selector: 'app-experience-page',
  standalone: true,
  imports: [
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
    ErrorStateComponent,
  ],
  template: `
    <app-page-header
      kicker="Career"
      title="Experience"
      subtitle="Where I've worked, what I built, and the stacks I built it with."
    />

    @if (loading()) {
      <app-loading-state />
    } @else if (error()) {
      <app-error-state />
    } @else if (entries().length === 0) {
      <app-empty-state />
    } @else {
      <ol class="relative space-y-8 border-l border-neutral-800 pl-8">
        @for (entry of entries(); track entry.company + entry.title) {
          <li class="relative">
            <span
              class="absolute -left-[41px] top-1.5 h-3 w-3 rounded-full border-2 border-amber-400 bg-neutral-950 transition-shadow"
              [class.bg-amber-400]="entry.isCurrent"
              [class.ring-4]="entry.isCurrent"
              [class.ring-amber-400/20]="entry.isCurrent"
              aria-hidden="true"
            ></span>
            <article class="card p-6">
              <div class="flex flex-wrap items-baseline justify-between gap-x-4 gap-y-1">
                <div>
                  <h2 class="text-lg font-semibold text-white">{{ entry.title }}</h2>
                  <p class="text-sm font-medium text-amber-400">{{ entry.company }}</p>
                </div>
                <div class="flex items-center gap-2 text-sm text-neutral-500">
                  <span class="tabular-nums">{{ dateRange(entry) }}</span>
                  @if (entry.isCurrent) {
                    <span class="chip chip-accent">Current</span>
                  }
                </div>
              </div>
              <p class="mt-3 text-sm leading-relaxed text-neutral-400">{{ entry.description }}</p>
              @if (entry.technologies.length > 0) {
                <div class="mt-4 flex flex-wrap gap-1.5">
                  @for (tech of entry.technologies; track tech) {
                    <span class="chip">{{ tech }}</span>
                  }
                </div>
              }
            </article>
          </li>
        }
      </ol>
    }
  `,
})
export class ExperiencePageComponent extends ServerSafePage implements OnInit {
  private readonly api = inject(PortfolioApiService);

  protected readonly entries = signal<readonly ExperienceDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal(false);

  ngOnInit(): void {
    this.loadOnBrowser(this.loading, this.error, this.api.getExperience(), (entries) => {
      this.entries.set(entries);
    });
  }

  protected dateRange(entry: ExperienceDto): string {
    const fmt = (value: string | null): string =>
      value
        ? new Date(value).toLocaleDateString(undefined, { year: 'numeric', month: 'short' })
        : '';
    const start = fmt(entry.startedOn);
    if (entry.isCurrent) {
      return `${start} — Present`;
    }
    return `${start} — ${fmt(entry.endedOn)}`;
  }
}