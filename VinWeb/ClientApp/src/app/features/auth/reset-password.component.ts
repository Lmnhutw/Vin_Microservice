import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <section class="form-card" aria-labelledby="reset-title">
      <p class="eyebrow">Account recovery</p>
      <h1 id="reset-title">Reset your password</h1>
      <form [formGroup]="forgotForm" (ngSubmit)="sendReset()" novalidate>
        <label for="forgot-email">Email</label>
        <input id="forgot-email" type="email" formControlName="email" autocomplete="email" />
        @if (forgotForm.controls.email.touched && forgotForm.controls.email.invalid) {
          <small class="error">Enter a valid email address.</small>
        }
        <button class="primary-button" type="submit" [disabled]="forgotForm.invalid || loading">Send reset instructions</button>
      </form>
      <hr />
      <p class="muted">Already have a reset token? Complete the form below.</p>
      <form [formGroup]="resetForm" (ngSubmit)="reset()" novalidate>
        <label for="reset-email">Email</label>
        <input id="reset-email" type="email" formControlName="email" autocomplete="email" />
        @if (resetForm.controls.email.touched && resetForm.controls.email.invalid) {
          <small class="error">Enter a valid email address.</small>
        }
        <label for="token">Reset token</label>
        <input id="token" formControlName="token" autocomplete="one-time-code" />
        @if (resetForm.controls.token.touched && resetForm.controls.token.invalid) {
          <small class="error">Enter the reset token from your message.</small>
        }
        <label for="new-password">New password</label>
        <input id="new-password" type="password" formControlName="newPassword" autocomplete="new-password" />
        @if (resetForm.controls.newPassword.touched && resetForm.controls.newPassword.invalid) {
          <small class="error">Use at least 8 characters.</small>
        }
        @if (message) { <p class="muted" role="status">{{ message }}</p> }
        @if (error) { <p class="error" role="alert">{{ error }}</p> }
        <button class="primary-button" type="submit" [disabled]="resetForm.invalid || loading">Reset password</button>
      </form>
      <p class="muted"><a routerLink="/login">Back to sign in</a></p>
    </section>
  `
})
export class ResetPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  protected readonly forgotForm = this.fb.nonNullable.group({ email: ['', [Validators.required, Validators.email]] });
  protected readonly resetForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]], token: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]]
  });
  protected loading = false;
  protected message = '';
  protected error = '';

  sendReset(): void {
    if (this.forgotForm.invalid) {
      this.forgotForm.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.error = '';
    this.message = '';
    this.api.forgotPassword(this.forgotForm.controls.email.value).subscribe({
      next: response => { this.loading = false; this.message = response.message || 'If the account exists, reset instructions will be sent.'; },
      error: () => { this.loading = false; this.error = 'Unable to send reset instructions.'; }
    });
  }

  reset(): void {
    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.error = '';
    this.message = '';
    const value = this.resetForm.getRawValue();
    this.api.resetPassword(value.email, value.token, value.newPassword).subscribe({
      next: response => { this.loading = false; this.message = response.message || 'Password reset successfully.'; },
      error: () => { this.loading = false; this.error = 'The reset token is invalid or expired.'; }
    });
  }
}
