import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminApiService } from '@core/services/admin-api.service';
import { AdminAuthService } from '@core/services/admin-auth.service';
import { ToastService } from '@core/services/toast.service';
import { toFieldErrors } from '@core/models/problem-details';
import { AdminBlogPostDto, UpsertBlogPostRequest } from '@shared/models/admin.models';

@Component({
  selector: 'app-admin-blog-page',
  imports: [ReactiveFormsModule],
  templateUrl: './blog-page.component.html',
})
export class AdminBlogPageComponent implements OnInit {
  private readonly api = inject(AdminApiService);
  private readonly auth = inject(AdminAuthService);
  private readonly toast = inject(ToastService);

  protected readonly items = signal<AdminBlogPostDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly editing = signal<'new' | 'edit' | null>(null);
  protected readonly saving = signal(false);
  protected readonly fieldErrors = signal<Record<string, string>>({});
  private editingId: string | null = null;

  protected readonly form = new FormGroup({
    slug: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(120)] }),
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    excerpt: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(400)] }),
    bodyMarkdown: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    coverImageUrl: new FormControl('', { nonNullable: true }),
    publishedAt: new FormControl('', { nonNullable: true }),
    readingMinutes: new FormControl(5, { nonNullable: true, validators: [Validators.required, Validators.min(1)] }),
    isPublished: new FormControl(false, { nonNullable: true }),
    tags: new FormControl('', { nonNullable: true }),
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
    this.api.getPosts().subscribe({
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
      excerpt: '',
      bodyMarkdown: '',
      coverImageUrl: '',
      publishedAt: '',
      readingMinutes: 5,
      isPublished: false,
      tags: '',
    });
    this.editing.set('new');
  }

  protected openEdit(post: AdminBlogPostDto): void {
    this.editingId = post.id;
    this.fieldErrors.set({});
    this.form.reset({
      slug: post.slug,
      title: post.title,
      excerpt: post.excerpt,
      bodyMarkdown: post.bodyMarkdown,
      coverImageUrl: post.coverImageUrl ?? '',
      publishedAt: post.publishedAt ? post.publishedAt.slice(0, 16) : '',
      readingMinutes: post.readingMinutes,
      isPublished: post.isPublished,
      tags: post.tags.join(', '),
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
    const request: UpsertBlogPostRequest = {
      slug: value.slug.trim(),
      title: value.title.trim(),
      excerpt: value.excerpt.trim(),
      bodyMarkdown: value.bodyMarkdown,
      coverImageUrl: value.coverImageUrl.trim() || null,
      publishedAt: value.publishedAt ? new Date(value.publishedAt).toISOString() : null,
      readingMinutes: value.readingMinutes,
      isPublished: value.isPublished,
      tags: value.tags
        .split(',')
        .map((tag) => tag.trim())
        .filter((tag) => tag.length > 0),
    };

    this.saving.set(true);
    const id = this.editingId;
    const request$ = id ? this.api.updatePost(id, request) : this.api.createPost(request);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success(id ? 'Post updated.' : 'Post created.');
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

  protected remove(post: AdminBlogPostDto): void {
    if (!confirm(`Delete "${post.title}"? This removes it from the public site.`)) {
      return;
    }
    this.api.deletePost(post.id).subscribe({
      next: () => {
        this.toast.success('Post deleted.');
        if (this.editingId === post.id) {
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
