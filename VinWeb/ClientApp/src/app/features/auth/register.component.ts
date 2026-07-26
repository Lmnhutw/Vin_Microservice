import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <section class="form-card" aria-labelledby="register-title">
      <p class="eyebrow">Join Vin Shop</p>
      <h1 id="register-title">Create your account</h1>
      <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <label for="fullName">Full name</label>
        <input id="fullName" formControlName="fullName" autocomplete="name" />
        @if (form.controls.fullName.touched && form.controls.fullName.invalid) {
          <small class="error">Enter your full name.</small>
        }
        <label for="email">Email</label>
        <input id="email" type="email" formControlName="email" autocomplete="email" />
        @if (form.controls.email.touched && form.controls.email.invalid) {
          <small class="error">Enter a valid email address.</small>
        }
        <label for="phone">Phone</label>
        <input id="phone" type="tel" formControlName="phoneNumber" autocomplete="tel" />
        @if (form.controls.phoneNumber.touched && form.controls.phoneNumber.invalid) {
          <small class="error">Enter a phone number.</small>
        }
        <label for="password">Password</label>
        <input id="password" type="password" formControlName="password" autocomplete="new-password" />
        @if (form.controls.password.touched && form.controls.password.invalid) {
          <small class="error">Use at least 8 characters.</small>
        }
        @if (error) { <p class="error" role="alert">{{ error }}</p> }
        <button class="primary-button" type="submit" [disabled]="form.invalid || loading">
          {{ loading ? 'Creating account…' : 'Create account' }}
        </button>
      </form>
      <p class="muted">Already have an account? <a routerLink="/login">Sign in</a>.</p>
    </section>
  `
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly form = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });
  protected loading = false;
  protected error = '';

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    const value = this.form.getRawValue();
    this.auth.register(value.email, value.fullName, value.phoneNumber, value.password).subscribe({
      next: () => this.router.navigateByUrl('/login'),
      error: () => { this.loading = false; this.error = 'Unable to create the account. Check the details and try again.'; }
    });
  }
}
