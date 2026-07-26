import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <section class="form-card" aria-labelledby="login-title">
      <p class="eyebrow">Welcome back</p>
      <h1 id="login-title">Sign in to Vin Shop</h1>
      <p class="muted">Access your cart and continue along the route you requested.</p>
      <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <label for="username">Email</label>
        <input id="username" type="email" formControlName="username" autocomplete="username" />
        @if (form.controls.username.touched && form.controls.username.invalid) {
          <small class="error">Enter a valid email address.</small>
        }
        <label for="password">Password</label>
        <input id="password" type="password" formControlName="password" autocomplete="current-password" />
        @if (form.controls.password.touched && form.controls.password.invalid) {
          <small class="error">Password is required.</small>
        }
        @if (error) { <p class="error" role="alert">{{ error }}</p> }
        <a class="form-side-link" routerLink="/reset-password">Forgot password?</a>
        <button class="primary-button" type="submit" [disabled]="form.invalid || loading">
          {{ loading ? 'Signing in…' : 'Sign in' }}
        </button>
      </form>
      <p class="muted">New to Vin Shop? <a routerLink="/register">Create an account</a>.</p>
    </section>
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  protected readonly form = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });
  protected loading = false;
  protected error = '';

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.error = '';
    const { username, password } = this.form.getRawValue();
    const requestedUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/';
    const returnUrl = requestedUrl.startsWith('/') && !requestedUrl.startsWith('//')
      ? requestedUrl
      : '/';
    this.auth.login(username, password).subscribe({
      next: () => this.router.navigateByUrl(returnUrl),
      error: () => {
        this.loading = false;
        this.error = 'Unable to sign in. Check your credentials and try again.';
      }
    });
  }
}
