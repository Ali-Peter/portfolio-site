import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminApiService } from '@core/services/admin-api.service';
import { AdminAuthService } from '@core/services/admin-auth.service';
import { ToastService } from '@core/services/toast.service';
import { toFieldErrors } from '@core/models/problem-details';
import { AdminSkillDto, UpsertSkillRequest } from '@shared/models/admin.models';
import { SkillCategory, SkillProficiency } from '@shared/models/api.models';

@Component({
  selector: 'app-admin-skills-page',
  imports: [ReactiveFormsModule],
  templateUrl: './skills-page.component.html',
})
export class AdminSkillsPageComponent implements OnInit {
  private readonly api = inject(AdminApiService);
  private readonly auth = inject(AdminAuthService);
  private readonly toast = inject(ToastService);

  protected readonly items = signal<AdminSkillDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly editing = signal<'new' | 'edit' | null>(null);
  protected readonly saving = signal(false);
  protected readonly fieldErrors = signal<Record<string, string>>({});
  private editingId: string | null = null;

  protected readonly categories: SkillCategory[] = [
    'Backend',
    'Frontend',
    'DataAndInfra',
    'AiMl',
    'Tools',
  ];
  protected readonly proficiencies: SkillProficiency[] = [
    'Beginner',
    'Intermediate',
    'Proficient',
    'Advanced',
    'Expert',
  ];

  protected readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(120)] }),
    category: new FormControl<SkillCategory>('Backend', { nonNullable: true }),
    proficiency: new FormControl<SkillProficiency>('Proficient', { nonNullable: true }),
    sortOrder: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
  });

  ngOnInit(): void {
    this.reload();
  }

  protected reload(): void {
    if (!this.auth.isAuthenticated()) {
      this.loading.set(false);
      return;
    }
    this.loading.set(true);
    this.api.getSkills().subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected openCreate(): void {
    this.editingId = null;
    this.fieldErrors.set({});
    this.form.reset({ name: '', category: 'Backend', proficiency: 'Proficient', sortOrder: 0 });
    this.editing.set('new');
  }

  protected openEdit(skill: AdminSkillDto): void {
    this.editingId = skill.id;
    this.fieldErrors.set({});
    this.form.reset({
      name: skill.name,
      category: skill.category,
      proficiency: skill.proficiency,
      sortOrder: skill.sortOrder,
    });
    this.editing.set('edit');
  }

  protected close(): void {
    this.editing.set(null);
    this.fieldErrors.set({});
  }

  protected save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    const request: UpsertSkillRequest = this.form.getRawValue();
    this.saving.set(true);
    const id = this.editingId;
    const request$ = id ? this.api.updateSkill(id, request) : this.api.createSkill(request);
    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success(id ? 'Skill updated.' : 'Skill created.');
        this.close();
        this.reload();
      },
      error: (error: unknown) => {
        this.saving.set(false);
        if (error instanceof HttpErrorResponse && error.status === 422) {
          this.fieldErrors.set(toFieldErrors(error.error));
        }
      },
    });
  }

  protected remove(skill: AdminSkillDto): void {
    if (!confirm(`Delete skill "${skill.name}"?`)) {
      return;
    }
    this.api.deleteSkill(skill.id).subscribe({
      next: () => {
        this.toast.success('Skill deleted.');
        if (this.editingId === skill.id) {
          this.close();
        }
        this.reload();
      },
      error: () => undefined,
    });
  }

  protected fieldError(name: string): string | undefined {
    return this.fieldErrors()[name];
  }
}
