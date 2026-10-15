import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { PortfolioApiService } from '@core/services/portfolio-api.service';
import { ServerSafePage } from '@shared/server-safe-page';
import { PageHeaderComponent } from '@shared/ui/page-header/page-header.component';
import {
  ErrorStateComponent,
  LoadingStateComponent,
} from '@shared/ui/list-states/list-states.component';
import { ProjectDto, ProjectStatus } from '@shared/models/api.models';

const STATUS_LABELS: Record<ProjectStatus, string> = {
  Planned: 'Planned',
  InProgress: 'In progress',
  Completed: 'Completed',
  Archived: 'Archived',
};

/** Single project. Unknown slugs render the not-found panel (API 404). */
@Component({
  selector: 'app-project-detail-page',
  standalone: true,
  imports: [PageHeaderComponent, RouterLink, LoadingStateComponent, ErrorStateComponent],
  template: `
    @if (loading()) {
      <app-loading-state />
    } @else if (project(); as p) {
      <a routerLink="/projects" class="text-sm text-neutral-500 transition hover:text-amber-300">
        ← All projects
      </a>

      <app-page-header class="mt-6" [title]="p.title" [subtitle]="p.summary" />

      @if (p.coverImageUrl) {
        <div class="card mb-8 overflow-hidden p-0">
          <img
            [src]="p.coverImageUrl"
            [alt]="p.title"
            class="aspect-[16/9] w-full object-cover"
          />
        </div>
      }

      <div class="grid gap-10 lg:grid-cols-[2fr_1fr] lg:gap-14">
        <div>
          <p class="whitespace-pre-line text-base leading-7 text-neutral-300">{{ p.description }}</p>

          <div class="mt-8 flex flex-wrap gap-3">
            @if (p.demoUrl) {
              <a
                [href]="p.demoUrl"
                target="_blank"
                rel="noopener noreferrer"
                class="btn btn-primary"
              >
                Live demo ↗
              </a>
            }
            @if (p.repoUrl) {
              <a
                [href]="p.repoUrl"
                target="_blank"
                rel="noopener noreferrer"
                class="btn btn-outline"
              >
                Source code ↗
              </a>
            }
          </div>
        </div>

        <aside class="card h-fit p-6 lg:sticky lg:top-24">
          <dl class="space-y-4 text-sm">
            <div>
              <dt class="text-neutral-500">Status</dt>
              <dd class="mt-1 font-medium text-white">{{ statusLabel(p.status) }}</dd>
            </div>
            @if (p.startedOn) {
              <div>
                <dt class="text-neutral-500">Timeline</dt>
                <dd class="mt-1 font-medium text-white">{{ timeline(p) }}</dd>
              </div>
            }
            <div>
              <dt class="text-neutral-500">Technologies</dt>
              <dd class="mt-2 flex flex-wrap gap-1.5">
                @for (tech of p.technologies; track tech) {
                  <span class="chip">{{ tech }}</span>
                }
              </dd>
            </div>
          </dl>
        </aside>
      </div>
    } @else if (notFound()) {
      <app-page-header title="Project not found" subtitle="No project uses that address." />
      <a routerLink="/projects" class="btn btn-outline">← Back to projects</a>
    } @else {
      <app-error-state />
    }
  `,
})
export class ProjectDetailPageComponent extends ServerSafePage implements OnInit {
  private readonly api = inject(PortfolioApiService);
  private readonly route = inject(ActivatedRoute);

  protected readonly project = signal<ProjectDto | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly loadError = signal(false);

  protected statusLabel(status: ProjectStatus): string {
    return STATUS_LABELS[status];
  }

  protected timeline(project: ProjectDto): string {
    const fmt = (value: string | null): string =>
      value
        ? new Date(value).toLocaleDateString(undefined, { year: 'numeric', month: 'short' })
        : '';
    const start = fmt(project.startedOn);
    const end = project.completedOn ? fmt(project.completedOn) : 'Present';
    return `${start} — ${end}`;
  }

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    if (!this.isBrowser) {
      this.loading.set(false);
      return;
    }
    this.api.getProjectBySlug(slug).subscribe({
      next: (project) => {
        this.project.set(project);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 404) {
          this.notFound.set(true);
        } else {
          this.loadError.set(true);
        }
        this.loading.set(false);
      },
    });
  }
}
