import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { AdminAuthService } from '@core/services/admin-auth.service';
import { HttpErrorResponse } from '@angular/common/http';

/** Full-screen admin login (outside the guarded shell so it stays reachable). */
@Component({
  selector: 'app-admin-login-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login-page.component.html',
})
export class AdminLoginPageComponent implements OnInit {
  private readonly auth = inject(AdminAuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected readonly returnUrl = computed(
    () => this.route.snapshot.queryParams['returnUrl'] ?? '/admin/dashboard',
  );

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      void this.router.navigateByUrl(this.returnUrl(), { replaceUrl: true });
    }
  }

  protected submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password).subscribe({
      next: () => {
        const target = this.returnUrl();
        void this.router.navigateByUrl(target, { replaceUrl: true });
      },
      error: (error: unknown) => {
        this.submitting.set(false);
        if (error instanceof HttpErrorResponse) {
          this.errorMessage.set(
            error.status === 401
              ? 'Invalid email or password.'
              : error.status === 429
                ? 'Too many attempts — please wait a minute and try again.'
                : 'Something went wrong signing you in. Please try again.',
          );
          return;
        }
        this.errorMessage.set('Something went wrong signing you in. Please try again.');
      },
    });
  }
}
