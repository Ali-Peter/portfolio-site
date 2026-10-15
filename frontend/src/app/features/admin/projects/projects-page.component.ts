import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminApiService } from '@core/services/admin-api.service';
import { AdminAuthService } from '@core/services/admin-auth.service';
import { ToastService } from '@core/services/toast.service';
import { toFieldErrors } from '@core/models/problem-details';
import { AdminProjectDto, UpsertProjectRequest } from '@shared/models/admin.models';
import { ProjectStatus } from '@shared/models/api.models';

@Component({
  selector: 'app-admin-projects-page',
  imports: [ReactiveFormsModule],
  templateUrl: './projects-page.component.html',
})
export class AdminProjectsPageComponent implements OnInit {
  private readonly api = inject(AdminApiService);
  private readonly auth = inject(AdminAuthService);
  private readonly toast = inject(ToastService);

  protected readonly items = signal<AdminProjectDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly editing = signal<'new' | 'edit' | null>(null);
  protected readonly saving = signal(false);
  protected readonly fieldErrors = signal<Record<string, string>>({});
  private editingId: string | null = null;

  protected readonly statuses: ProjectStatus[] = [
    'Planned',
    'InProgress',
    'Completed',
    'Archived',
  ];
  protected readonly statusLabels: Record<ProjectStatus, string> = {
    Planned: 'Planned',
    InProgress: 'In progress',
    Completed: 'Completed',
    Archived: 'Archived',
  };

  protected readonly form = new FormGroup({
    slug: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(120)] }),
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    summary: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(400)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    repoUrl: new FormControl('', { nonNullable: true }),
    demoUrl: new FormControl('', { nonNullable: true }),
    coverImageUrl: new FormControl('', { nonNullable: true }),
    status: new FormControl<ProjectStatus>('Completed', { nonNullable: true }),
    featured: new FormControl(false, { nonNullable: true }),
    startedOn: new FormControl('', { nonNullable: true }),
    completedOn: new FormControl('', { nonNullable: true }),
    sortOrder: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
    technologies: new FormControl('', { nonNullable: true }),
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
    this.api.getProjects().subscribe({
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
      slug: '',
      title: '',
      summary: '',
      description: '',
      repoUrl: '',
      demoUrl: '',
      coverImageUrl: '',
      status: 'Completed',
      featured: false,
      startedOn: '',
      completedOn: '',
      sortOrder: 0,
      technologies: '',
    });
    this.editing.set('new');
  }

  protected openEdit(project: AdminProjectDto): void {
    this.editingId = project.id;
    this.fieldErrors.set({});
    this.form.reset({
      slug: project.slug,
      title: project.title,
      summary: project.summary,
      description: project.description,
      repoUrl: project.repoUrl ?? '',
      demoUrl: project.demoUrl ?? '',
      coverImageUrl: project.coverImageUrl ?? '',
      status: project.status,
      featured: project.featured,
      startedOn: project.startedOn ?? '',
      completedOn: project.completedOn ?? '',
      sortOrder: project.sortOrder,
      technologies: project.technologies.join(', '),
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
    const request: UpsertProjectRequest = {
      slug: value.slug.trim(),
      title: value.title.trim(),
      summary: value.summary.trim(),
      description: value.description,
      repoUrl: value.repoUrl.trim() || null,
      demoUrl: value.demoUrl.trim() || null,
      coverImageUrl: value.coverImageUrl.trim() || null,
      status: value.status,
      featured: value.featured,
      startedOn: value.startedOn || null,
      completedOn: value.completedOn || null,
      sortOrder: value.sortOrder,
      technologies: splitList(value.technologies),
    };

    this.saving.set(true);
    const id = this.editingId;
    const request$ = id
      ? this.api.updateProject(id, request)
      : this.api.createProject(request);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success(id ? 'Project updated.' : 'Project created.');
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

  protected remove(project: AdminProjectDto): void {
    if (!confirm(`Delete "${project.title}"? This removes it from the public site.`)) {
      return;
    }
    this.api.deleteProject(project.id).subscribe({
      next: () => {
        this.toast.success('Project deleted.');
        if (this.editingId === project.id) {
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

function splitList(raw: string): string[] {
  return raw
    .split(',')
    .map((entry) => entry.trim())
    .filter((entry) => entry.length > 0);
}
