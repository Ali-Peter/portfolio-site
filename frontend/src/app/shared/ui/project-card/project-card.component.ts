import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProjectDto, ProjectStatus } from '@shared/models/api.models';

const STATUS_LABELS: Record<ProjectStatus, string> = {
  Planned: 'Planned',
  InProgress: 'In progress',
  Completed: 'Completed',
  Archived: 'Archived',
};

const STATUS_CLASSES: Record<ProjectStatus, string> = {
  Planned: 'border-neutral-600/80 bg-neutral-800/80 text-neutral-300',
  InProgress: 'border-amber-500/40 bg-amber-500/15 text-amber-300',
  Completed: 'border-emerald-700/60 bg-emerald-950/80 text-emerald-300',
  Archived: 'border-neutral-700/80 bg-neutral-800/80 text-neutral-400',
};

/** Reusable project card used by the home page and the projects grid. */
@Component({
  selector: 'app-project-card',
  standalone: true,
  imports: [RouterLink],
  template: `
    <a
      [routerLink]="['/projects', project().slug]"
      class="card card-hover group flex h-full flex-col overflow-hidden"
    >
      <div class="relative aspect-[16/9] overflow-hidden bg-neutral-900">
        @if (project().coverImageUrl) {
          <img
            [src]="project().coverImageUrl"
            [alt]="project().title"
            loading="lazy"
            class="h-full w-full object-cover transition duration-300 group-hover:scale-[1.03]"
          />
        } @else {
          <div class="flex h-full items-center justify-center bg-linear-to-br from-neutral-900 via-neutral-950 to-amber-950/30">
            <span class="select-none font-display text-6xl font-bold text-neutral-700/90">
              {{ project().title.charAt(0) }}
            </span>
          </div>
        }
        <div class="absolute inset-x-0 bottom-0 h-16 bg-linear-to-t from-neutral-950/80 to-transparent"></div>
        <span class="absolute right-3 top-3 rounded-full border px-2.5 py-0.5 text-xs font-medium backdrop-blur-sm {{ statusClass() }}">
          {{ statusLabel() }}
        </span>
        @if (project().featured) {
          <span class="chip chip-accent absolute left-3 top-3 border-amber-400/60 backdrop-blur-sm">★ Featured</span>
        }
      </div>

      <div class="flex flex-1 flex-col p-5">
        <h3 class="text-lg font-semibold text-white transition group-hover:text-amber-300">
          {{ project().title }}
        </h3>
        <p class="mt-2 line-clamp-2 flex-1 text-sm leading-relaxed text-neutral-400">
          {{ project().summary }}
        </p>
        <div class="mt-4 flex flex-wrap gap-1.5">
          @for (tech of project().technologies.slice(0, 5); track tech) {
            <span class="chip">{{ tech }}</span>
          }
          @if (project().technologies.length > 5) {
            <span class="chip border-transparent bg-transparent text-neutral-500">
              +{{ project().technologies.length - 5 }}
            </span>
          }
        </div>
      </div>
    </a>
  `,
})
export class ProjectCardComponent {
  readonly project = input.required<ProjectDto>();

  statusLabel(): string {
    return STATUS_LABELS[this.project().status];
  }

  statusClass(): string {
    return STATUS_CLASSES[this.project().status];
  }
}