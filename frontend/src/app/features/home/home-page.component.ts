import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PortfolioApiService } from '@core/services/portfolio-api.service';
import { ServerSafePage } from '@shared/server-safe-page';
import {
  EmptyStateComponent,
  ErrorStateComponent,
  LoadingStateComponent,
} from '@shared/ui/list-states/list-states.component';
import { ProjectCardComponent } from '@shared/ui/project-card/project-card.component';
import {
  ExperienceDto,
  ProjectDto,
  SkillCategory,
  SkillDto,
} from '@shared/models/api.models';

/** Skills grouped under a category heading for the home page. */
interface SkillGroup {
  category: SkillCategory;
  label: string;
  skills: SkillDto[];
}

const CATEGORY_LABELS: Record<SkillCategory, string> = {
  Backend: 'Backend',
  Frontend: 'Frontend',
  DataAndInfra: 'Data & Infrastructure',
  AiMl: 'AI / ML',
  Tools: 'Tooling',
};

/**
 * Landing page: hero with portrait, featured projects, skills preview,
 * experience preview and a closing CTA — all from the API.
 */
@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [
    RouterLink,
    LoadingStateComponent,
    EmptyStateComponent,
    ErrorStateComponent,
    ProjectCardComponent,
  ],
  template: `
    <!-- Hero: portrait stacks first on mobile (image → name → intro → CTAs);
         explicit placement keeps copy left / portrait right on the lg grid. -->
    <section
      class="flex flex-col items-center gap-10 text-center lg:grid lg:grid-cols-[1.6fr_1fr] lg:gap-14 lg:text-left"
    >
      <div class="relative mx-auto w-full max-w-[16rem] sm:max-w-sm lg:col-start-2 lg:row-start-1">
        <div
          class="absolute -inset-4 rounded-[2.5rem] bg-linear-to-tr from-amber-500/25 via-transparent to-sky-500/15 blur-2xl"
          aria-hidden="true"
        ></div>
        <div class="relative rounded-[1.75rem] border border-neutral-800 bg-neutral-900/70 p-2 shadow-2xl shadow-black/50">
          <img
            src="assets/profile.png"
            alt="Portrait of Ali Ogochukwu Peter"
            class="aspect-square w-full rounded-[1.25rem] object-cover"
          />
        </div>
      </div>
      <div class="w-full lg:col-start-1 lg:row-start-1">
        <p class="kicker mb-4 lg:mb-5">Software &amp; AI Engineer</p>
        <h1 class="text-4xl font-bold leading-[1.1] tracking-tight text-white text-balance sm:text-5xl">
          Ali Ogochukwu Peter
        </h1>
        <p class="mx-auto mt-6 max-w-xl text-lg leading-relaxed text-neutral-400 lg:mx-0">
          I build typed backends with <span class="text-neutral-200">ASP.NET Core</span>,
          modern frontends with <span class="text-neutral-200">Angular</span>, and applied LLM
          systems — all backed by <span class="text-neutral-200">PostgreSQL</span> and automated
          tests.
        </p>
        <div class="mt-8 flex flex-wrap justify-center gap-3 lg:mt-9 lg:justify-start">
          <a routerLink="/projects" class="btn btn-primary">View projects</a>
          <a routerLink="/contact" class="btn btn-outline">Get in touch</a>
          <a routerLink="/ai-assistant" class="btn btn-outline">Ask my AI assistant</a>
        </div>
        <div class="mt-8 flex flex-wrap items-center justify-center gap-x-5 gap-y-2 border-t border-neutral-800/80 pt-6 text-sm text-neutral-500 lg:mt-9 lg:justify-start">
          <a
            href="https://github.com/Ali-Peter"
            target="_blank"
            rel="noopener noreferrer"
            class="transition hover:text-amber-300"
            >GitHub →</a
          >
          <a
            href="https://dev.to/ali-peter"
            target="_blank"
            rel="noopener noreferrer"
            class="transition hover:text-amber-300"
            >Dev.to →</a
          >
          <a
            href="https://x.com/ali_ogochu2581"
            target="_blank"
            rel="noopener noreferrer"
            class="transition hover:text-amber-300"
            >X / Twitter →</a
          >
          <a href="mailto:aliogochukwu06@gmail.com" class="transition hover:text-amber-300"
            >aliogochukwu06@gmail.com</a
          >
        </div>
      </div>
    </section>

    <!-- Featured projects -->
    <section aria-label="Featured projects" class="mt-20">
      <div class="mb-6 flex items-end justify-between gap-4">
        <div>
          <p class="kicker mb-2">Selected work</p>
          <h2 class="text-2xl font-semibold text-white">Featured projects</h2>
        </div>
        <a routerLink="/projects" class="text-sm text-amber-400 transition hover:text-amber-300">
          All projects →
        </a>
      </div>
      @if (projectsLoading()) {
        <app-loading-state />
      } @else if (projectsError()) {
        <app-error-state />
      } @else if (featured().length === 0) {
        <app-empty-state />
      } @else {
        <div class="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          @for (project of featured(); track project.slug) {
            <app-project-card [project]="project" />
          }
        </div>
      }
    </section>
    <!-- Skills preview -->
    <section aria-label="Skills" class="mt-20">
      <div class="mb-6 flex items-end justify-between gap-4">
        <div>
          <p class="kicker mb-2">Toolkit</p>
          <h2 class="text-2xl font-semibold text-white">Core skills</h2>
        </div>
        <a routerLink="/skills" class="text-sm text-amber-400 transition hover:text-amber-300">
          Full skill set →
        </a>
      </div>
      @if (skillsLoading()) {
        <app-loading-state />
      } @else if (skillsError()) {
        <app-error-state />
      } @else if (skillGroups().length === 0) {
        <app-empty-state />
      } @else {
        <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          @for (group of skillGroups(); track group.category) {
            <div class="card p-5">
              <h3 class="text-[13px] font-semibold uppercase tracking-wider text-neutral-400">{{ group.label }}</h3>
              <div class="mt-3 flex flex-wrap gap-1.5">
                @for (skill of group.skills.slice(0, 8); track skill.name) {
                  <span class="chip">{{ skill.name }}</span>
                }
                @if (group.skills.length > 8) {
                  <span class="chip border-transparent bg-transparent text-neutral-500">
                    +{{ group.skills.length - 8 }}
                  </span>
                }
              </div>
            </div>
          }
        </div>
      }
    </section>

    <!-- Experience preview -->
    <section aria-label="Experience" class="mt-20">
      <div class="mb-6 flex items-end justify-between gap-4">
        <div>
          <p class="kicker mb-2">Career</p>
          <h2 class="text-2xl font-semibold text-white">Experience</h2>
        </div>
        <a routerLink="/experience" class="text-sm text-amber-400 transition hover:text-amber-300">
          Full history →
        </a>
      </div>
      @if (experienceLoading()) {
        <app-loading-state />
      } @else if (experienceError()) {
        <app-error-state />
      } @else if (experience().length === 0) {
        <app-empty-state />
      } @else {
        <ol class="relative space-y-6 border-l border-neutral-800 pl-8">
          @for (entry of experience().slice(0, 3); track entry.company + entry.title) {
            <li class="relative">
              <span
                class="absolute -left-[41px] top-1.5 h-3 w-3 rounded-full border-2 border-amber-400 bg-neutral-950"
                [class.bg-amber-400]="entry.isCurrent"
                aria-hidden="true"
              ></span>
              <div class="card p-5">
                <div class="flex flex-wrap items-baseline justify-between gap-x-4 gap-y-1">
                  <h3 class="font-semibold text-white">{{ entry.title }}</h3>
                  <span class="text-xs text-neutral-500">
                    {{ dateRange(entry) }}
                    @if (entry.isCurrent) {
                      <span class="chip chip-accent ml-2 py-0">Current</span>
                    }
                  </span>
                </div>
                <p class="text-sm font-medium text-amber-400">{{ entry.company }}</p>
                <p class="mt-2 line-clamp-2 text-sm leading-relaxed text-neutral-400">
                  {{ entry.description }}
                </p>
              </div>
            </li>
          }
        </ol>
      }
    </section>
    <!-- CTA -->
    <section class="card relative mt-20 overflow-hidden border-amber-500/25 p-8 text-center sm:p-14">
      <div
        class="pointer-events-none absolute inset-0 bg-[radial-gradient(120%_120%_at_50%_0%,rgba(251,191,36,0.12),transparent_60%)]"
        aria-hidden="true"
      ></div>
      <div class="relative">
        <p class="kicker mb-3">Next step</p>
        <h2 class="text-2xl font-semibold tracking-tight text-white text-balance sm:text-3xl">
          Have a project in mind?
        </h2>
        <p class="mx-auto mt-4 max-w-xl leading-relaxed text-neutral-400">
          I'm open to freelance work and full-time roles in backend, full-stack and AI engineering.
        </p>
        <div class="mt-8 flex flex-wrap justify-center gap-3">
          <a routerLink="/contact" class="btn btn-primary">Start a conversation</a>
          <a routerLink="/blog" class="btn btn-outline">Read the blog</a>
        </div>
      </div>
    </section>
  `,
})
export class HomePageComponent extends ServerSafePage implements OnInit {
  private readonly api = inject(PortfolioApiService);

  protected readonly featured = signal<readonly ProjectDto[]>([]);
  protected readonly projectsLoading = signal(true);
  protected readonly projectsError = signal(false);

  protected readonly skillGroups = signal<readonly SkillGroup[]>([]);
  protected readonly skillsLoading = signal(true);
  protected readonly skillsError = signal(false);

  protected readonly experience = signal<readonly ExperienceDto[]>([]);
  protected readonly experienceLoading = signal(true);
  protected readonly experienceError = signal(false);

  ngOnInit(): void {
    this.loadOnBrowser(
      this.projectsLoading,
      this.projectsError,
      this.api.getProjects(1, 20),
      (page) => {
        const featured = page.items.filter((p) => p.featured);
        this.featured.set(featured.length > 0 ? featured.slice(0, 6) : page.items.slice(0, 3));
      },
    );

    this.loadOnBrowser(this.skillsLoading, this.skillsError, this.api.getSkills(), (skills) => {
      this.skillGroups.set(groupSkills(skills));
    });

    this.loadOnBrowser(
      this.experienceLoading,
      this.experienceError,
      this.api.getExperience(),
      (entries) => {
        this.experience.set(entries);
      },
    );
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

/** Bucket a flat, category-ordered skill list into groups preserving order. */
function groupSkills(skills: readonly SkillDto[]): SkillGroup[] {
  const groups: SkillGroup[] = [];
  for (const skill of skills) {
    let group = groups.find((g) => g.category === skill.category);
    if (!group) {
      group = {
        category: skill.category,
        label: CATEGORY_LABELS[skill.category] ?? skill.category,
        skills: [],
      };
      groups.push(group);
    }
    group.skills.push(skill);
  }
  return groups;
}
