import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {
  FormControl,
  FormGroup,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { PortfolioApiService } from '@core/services/portfolio-api.service';
import { ToastService } from '@core/services/toast.service';
import { ProblemDetails } from '@core/models/problem-details';
import { PageHeaderComponent } from '@shared/ui/page-header/page-header.component';
import { CONTACT_FORM_TEMPLATE } from './contact-page.template';

interface ContactForm {
  name: FormControl<string>;
  email: FormControl<string>;
  message: FormControl<string>;
  website: FormControl<string>;
}

@Component({
  selector: 'app-contact-page',
  standalone: true,
  imports: [PageHeaderComponent, ReactiveFormsModule],
  template: CONTACT_FORM_TEMPLATE,
})
export class ContactPageComponent {
  private readonly api = inject(PortfolioApiService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly form: FormGroup<ContactForm> = this.fb.group({
    name: this.fb.control('', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]),
    email: this.fb.control('', [Validators.required, Validators.email, Validators.maxLength(320)]),
    message: this.fb.control('', [Validators.required, Validators.minLength(10), Validators.maxLength(5000)]),
    website: this.fb.control(''),
  });

  protected readonly sending = signal(false);
  protected readonly sent = signal(false);
  protected readonly serverErrors = signal<Record<string, string[]>>({});

  protected showError(field: keyof ContactForm): boolean {
    const control = this.form.controls[field];
    return (
      (control.invalid && (control.touched || control.dirty)) ||
      (this.serverErrors()[this.serverKey(field)]?.length ?? 0) > 0
    );
  }

  protected fieldError(field: keyof ContactForm): string {
    const server = this.serverErrors()[this.serverKey(field)];
    if (server?.length) {
      return server[0];
    }
    const control = this.form.controls[field];
    if (control.hasError('required')) {
      return 'This field is required.';
    }
    if (control.hasError('email')) {
      return 'Enter a valid email address.';
    }
    if (control.hasError('minlength')) {
      const required = control.getError('minlength')?.requiredLength ?? 0;
      return `Enter at least ${required} characters.`;
    }
    if (control.hasError('maxlength')) {
      return 'That is too long.';
    }
    return 'This field is invalid.';
  }

  protected submit(): void {
    if (this.sending()) {
      return;
    }
    this.serverErrors.set({});
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.sending.set(true);
    const { name, email, message, website } = this.form.getRawValue();
    this.api.submitContact({ name, email, message, website: website || null }).subscribe({
      next: () => {
        this.sent.set(true);
        this.sending.set(false);
        this.toast.success('Message sent — thank you.');
      },
      error: (error: unknown) => {
        this.sending.set(false);
        if (error instanceof HttpErrorResponse && error.status === 422) {
          const errors = (error.error as ProblemDetails | null)?.errors ?? {};
          this.serverErrors.set(errors as Record<string, string[]>);
        }
      },
    });
  }

  private serverKey(field: keyof ContactForm): string {
    return field.charAt(0).toUpperCase() + field.slice(1);
  }
}
