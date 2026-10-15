import { Component, OnInit, inject, signal } from '@angular/core';
import { PortfolioApiService } from '@core/services/portfolio-api.service';
import { ServerSafePage } from '@shared/server-safe-page';
import { PageHeaderComponent } from '@shared/ui/page-header/page-header.component';
import {
  EmptyStateComponent,
  ErrorStateComponent,
  LoadingStateComponent,
} from '@shared/ui/list-states/list-states.component';
import { SkillCategory, SkillDto, SkillProficiency } from '@shared/models/api.models';

/** Skills grouped under a category heading. */
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

/** One filled dot per proficiency level (1–5). */
const PROFICIENCY_LEVELS: Record<SkillProficiency, number> = {
  Beginner: 1,
  Intermediate: 2,
  Proficient: 3,
  Advanced: 4,
  Expert: 5,
};

/** Full skills catalogue grouped by category with proficiency meters. */
@Component({
  selector: 'app-skills-page',
  standalone: true,
  imports: [
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
    ErrorStateComponent,
  ],
  template: `
    <app-page-header
      kicker="Toolkit"
      title="Skills"
      subtitle="The technologies I reach for in production — grouped by where they live in the stack."
    />

    @if (loading()) {
      <app-loading-state />
    } @else if (error()) {
      <app-error-state />
    } @else if (groups().length === 0) {
      <app-empty-state />
    } @else {
      <div class="space-y-12">
        @for (group of groups(); track group.category) {
          <section [attr.aria-label]="group.label">
            <div class="mb-6 flex items-baseline justify-between gap-4 border-b border-neutral-800/80 pb-4">
              <h2 class="text-xl font-semibold tracking-tight text-white">{{ group.label }}</h2>
              <span class="text-xs font-medium tracking-wide text-neutral-500">{{ group.skills.length }} skills</span>
            </div>
            <div class="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
              @for (skill of group.skills; track skill.name) {
                <div class="card p-4">
                  <div class="flex items-center justify-between gap-3">
                    <span class="text-sm font-medium text-white">{{ skill.name }}</span>
                    <span
                      class="flex shrink-0 gap-1"
                      role="img"
                      [attr.aria-label]="'Proficiency: ' + skill.proficiency"
                    >
                      @for (dot of dots(skill.proficiency); track dot) {
                        <span
                          class="h-1.5 w-3.5 rounded-full transition-colors duration-300"
                          [class.bg-amber-400]="dot <= level(skill.proficiency)"
                          [class.bg-neutral-700/70]="dot > level(skill.proficiency)"
                        ></span>
                      }
                    </span>
                  </div>
                </div>
              }
            </div>
          </section>
        }
      </div>
    }
  `,
})
export class SkillsPageComponent extends ServerSafePage implements OnInit {
  private readonly api = inject(PortfolioApiService);

  protected readonly groups = signal<readonly SkillGroup[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal(false);

  ngOnInit(): void {
    this.loadOnBrowser(this.loading, this.error, this.api.getSkills(), (skills) => {
      this.groups.set(groupSkills(skills));
    });
  }

  protected dots(_proficiency: SkillProficiency): number[] {
    return [1, 2, 3, 4, 5];
  }

  protected level(proficiency: SkillProficiency): number {
    return PROFICIENCY_LEVELS[proficiency];
  }
}

/** Bucket a flat, category-ordered skill list into groups preserving order. */
export function groupSkills(skills: readonly SkillDto[]): SkillGroup[] {
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