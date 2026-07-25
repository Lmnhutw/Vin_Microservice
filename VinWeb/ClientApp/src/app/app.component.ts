import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'vin-root',
  standalone: true,
  imports: [AsyncPipe, RouterLink, RouterOutlet],
  template: `
    <header class="site-header">
      <a class="brand" routerLink="/">Vin Shop</a>
      <nav aria-label="Main navigation">
        <a routerLink="/">Shop</a>
        <a routerLink="/cart">Cart</a>
        @if (auth.user$ | async; as user) {
          <span class="welcome">Hello, {{ user.fullName || user.email }}</span>
          <button class="link-button" type="button" (click)="logout()">Log out</button>
        } @else {
          <a routerLink="/login">Sign in</a>
          <a routerLink="/register">Create account</a>
        }
      </nav>
    </header>
    <main class="page-shell"><router-outlet /></main>
  `
})
export class AppComponent {
  protected readonly auth = inject(AuthService);

  constructor() {
    this.auth.load().subscribe();
  }

  logout(): void {
    this.auth.logout().subscribe();
  }
}
