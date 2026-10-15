import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { PortfolioApiService } from '@core/services/portfolio-api.service';
import { ServerSafePage } from '@shared/server-safe-page';
import { PageHeaderComponent } from '@shared/ui/page-header/page-header.component';
import {
  EmptyStateComponent,
  ErrorStateComponent,
  LoadingStateComponent,
} from '@shared/ui/list-states/list-states.component';
import { ProjectCardComponent } from '@shared/ui/project-card/project-card.component';
import { ProjectDto } from '@shared/models/api.models';

/** Full catalogue with one-click technology tag filters. */
@Component({
  selector: 'app-projects-page',
  standalone: true,
  imports: [
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
    ErrorStateComponent,
    ProjectCardComponent,
  ],
  template: `
    <app-page-header
      kicker="Portfolio"
      title="Projects"
      subtitle="Selected work across enterprise ERP, AI voice automation, e-commerce and learning platforms."
    />

    @if (allTechnologies().length > 0) {
      <div class="mb-9 flex flex-wrap items-center gap-2">
        <span class="mr-1 text-xs font-semibold uppercase tracking-[0.18em] text-neutral-500">Filter</span>
        <button
          type="button"
          (click)="selectTag('')"
          class="{{ tag() === '' ? 'chip chip-accent' : 'chip' }} cursor-pointer px-3 py-1 transition hover:border-amber-500/50 hover:text-amber-300"
        >
          All
        </button>
        @for (tech of allTechnologies(); track tech) {
          <button
            type="button"
            (click)="selectTag(tech)"
            class="{{ tag() === tech ? 'chip chip-accent' : 'chip' }} cursor-pointer px-3 py-1 transition hover:border-amber-500/50 hover:text-amber-300"
          >
            {{ tech }}
          </button>
        }
      </div>
    }

    @if (loading()) {
      <app-loading-state />
    } @else if (error()) {
      <app-error-state />
    } @else if (visibleProjects().length === 0) {
      <app-empty-state />
    } @else {
      <div class="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        @for (project of visibleProjects(); track project.slug) {
          <app-project-card [project]="project" />
        }
      </div>
      <p class="mt-8 text-sm text-neutral-500">
        Showing {{ visibleProjects().length }} of {{ totalCount() }} projects.
      </p>
    }
  `,
})
export class ProjectsPageComponent extends ServerSafePage implements OnInit {
  private readonly api = inject(PortfolioApiService);

  protected readonly projects = signal<readonly ProjectDto[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly loading = signal(true);
  protected readonly error = signal(false);
  protected readonly tag = signal('');

  /** Unique technologies across all projects, alphabetical — powers the chips. */
  protected readonly allTechnologies = computed(() =>
    [...new Set(this.projects().flatMap((p) => p.technologies))].sort((a, b) =>
      a.localeCompare(b),
    ),
  );

  /** Projects shown after applying the selected tag. */
  protected readonly visibleProjects = computed(() => {
    const tag = this.tag();
    if (!tag) {
      return this.projects();
    }
    return this.projects().filter((p) => p.technologies.includes(tag));
  });

  ngOnInit(): void {
    this.load();
  }

  protected selectTag(tag: string): void {
    this.tag.set(tag);
  }

  private load(): void {
    this.loadOnBrowser(this.loading, this.error, this.api.getProjects(1, 50), (page) => {
      this.projects.set(page.items);
      this.totalCount.set(page.totalCount);
    });
  }
}