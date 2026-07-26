import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from './core/auth.service';
import { NotificationService } from './core/notification.service';

@Component({
  selector: 'vin-root',
  standalone: true,
  imports: [AsyncPipe, RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <header class="site-header">
      <a class="brand" routerLink="/" (click)="closeMenu()" aria-label="Vin Shop home">
        <span>VIN</span><small>SHOP</small>
      </a>
      <button
        class="menu-toggle"
        type="button"
        (click)="menuOpen = !menuOpen"
        [attr.aria-expanded]="menuOpen"
        aria-controls="primary-navigation"
      >
        <span></span><span></span>
        <span class="sr-only">Toggle navigation</span>
      </button>
      <nav id="primary-navigation" [class.is-open]="menuOpen" aria-label="Main navigation">
        <div class="nav-primary">
          <a routerLink="/" [routerLinkActiveOptions]="{ exact: true }" routerLinkActive="is-active" (click)="closeMenu()">Catalogue</a>
          <a routerLink="/cart" routerLinkActive="is-active" (click)="closeMenu()">Cart</a>
        </div>
        <div class="nav-account">
          @if (auth.user$ | async; as user) {
            @if (user.roles?.includes('ADMIN')) {
              <a routerLink="/admin" routerLinkActive="is-active" (click)="closeMenu()">Admin</a>
            }
            <span class="account-name">{{ user.fullName || user.name || user.email }}</span>
            <button class="text-action text-action--light" type="button" (click)="logout()" [disabled]="loggingOut">
              {{ loggingOut ? 'Signing out…' : 'Sign out' }}
            </button>
          } @else {
            <a routerLink="/login" routerLinkActive="is-active" (click)="closeMenu()">Sign in</a>
            <a class="nav-cta" routerLink="/register" (click)="closeMenu()">Create account</a>
          }
        </div>
      </nav>
    </header>
    <main><router-outlet /></main>

    <footer class="site-footer">
      <a class="footer-brand" routerLink="/"><span>VIN</span> SHOP</a>
      <p>A working storefront and an inspectable microservice learning project.</p>
      <div>
        <a routerLink="/">Catalogue</a>
        <a routerLink="/cart">Cart</a>
        <a href="/health">System health</a>
      </div>
      <small>© {{ currentYear }} Vin Shop</small>
    </footer>

    @if (notifications.notification(); as notice) {
      <div
        class="app-toast"
        [class]="'app-toast app-toast--' + notice.tone"
        role="status"
        aria-live="polite"
      >
        <span>{{ notice.message }}</span>
        <button type="button" (click)="notifications.dismiss()" aria-label="Dismiss notification">×</button>
      </div>
    }
  `
})
export class AppComponent {
  protected readonly auth = inject(AuthService);
  protected readonly notifications = inject(NotificationService);
  protected readonly currentYear = new Date().getFullYear();
  protected menuOpen = false;
  protected loggingOut = false;

  constructor() {
    this.auth.load().subscribe();
  }

  logout(): void {
    if (this.loggingOut) return;
    this.loggingOut = true;
    this.auth.logout().pipe(
      finalize(() => this.loggingOut = false)
    ).subscribe({
      next: () => {
        this.closeMenu();
        this.notifications.show('You are signed out.', 'info');
      },
      error: () => this.notifications.show('Sign out could not be completed. Try again.', 'error')
    });
  }

  closeMenu(): void {
    this.menuOpen = false;
  }
}
