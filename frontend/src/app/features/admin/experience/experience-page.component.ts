import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminApiService } from '@core/services/admin-api.service';
import { AdminAuthService } from '@core/services/admin-auth.service';
import { ToastService } from '@core/services/toast.service';
import { toFieldErrors } from '@core/models/problem-details';
import { AdminExperienceDto, UpsertExperienceRequest } from '@shared/models/admin.models';

@Component({
  selector: 'app-admin-experience-page',
  imports: [ReactiveFormsModule],
  templateUrl: './experience-page.component.html',
})
export class AdminExperiencePageComponent implements OnInit {
  private readonly api = inject(AdminApiService);
  private readonly auth = inject(AdminAuthService);
  private readonly toast = inject(ToastService);

  protected readonly items = signal<AdminExperienceDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly editing = signal<'new' | 'edit' | null>(null);
  protected readonly saving = signal(false);
  protected readonly fieldErrors = signal<Record<string, string>>({});
  private editingId: string | null = null;

  protected readonly form = new FormGroup({
    company: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    technologies: new FormControl('', { nonNullable: true }),
    startedOn: new FormControl('', { nonNullable: true }),
    endedOn: new FormControl('', { nonNullable: true }),
    isCurrent: new FormControl(false, { nonNullable: true }),
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
    this.api.getExperiences().subscribe({
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
    this.form.reset({
      company: '',
      title: '',
      description: '',
      technologies: '',
      startedOn: '',
      endedOn: '',
      isCurrent: false,
      sortOrder: 0,
    });
    this.editing.set('new');
  }

  protected openEdit(entry: AdminExperienceDto): void {
    this.editingId = entry.id;
    this.fieldErrors.set({});
    this.form.reset({
      company: entry.company,
      title: entry.title,
      description: entry.description,
      technologies: entry.technologies.join(', '),
      startedOn: entry.startedOn ?? '',
      endedOn: entry.endedOn ?? '',
      isCurrent: entry.isCurrent,
      sortOrder: entry.sortOrder,
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
    const value = this.form.getRawValue();
    const request: UpsertExperienceRequest = {
      company: value.company.trim(),
      title: value.title.trim(),
      description: value.description,
      technologies: value.technologies
        .split(',')
        .map((entry) => entry.trim())
        .filter((entry) => entry.length > 0),
      startedOn: value.startedOn || null,
      endedOn: value.isCurrent ? null : value.endedOn || null,
      isCurrent: value.isCurrent,
      sortOrder: value.sortOrder,
    };

    this.saving.set(true);
    const id = this.editingId;
    const request$ = id
      ? this.api.updateExperience(id, request)
      : this.api.createExperience(request);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success(id ? 'Experience updated.' : 'Experience created.');
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

  protected remove(entry: AdminExperienceDto): void {
    if (!confirm(`Delete "${entry.title} @ ${entry.company}"?`)) {
      return;
    }
    this.api.deleteExperience(entry.id).subscribe({
      next: () => {
        this.toast.success('Experience deleted.');
        if (this.editingId === entry.id) {
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
