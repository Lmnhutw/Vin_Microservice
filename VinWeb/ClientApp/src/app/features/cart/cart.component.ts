import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Cart } from '../../core/models';

@Component({
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  template: `
    <section class="section-heading">
      <p class="eyebrow">Your selection</p>
      <h1>Shopping cart</h1>
    </section>
    @if (error) { <p class="error" role="alert">{{ error }}</p> }
    @if (loading) { <p class="muted">Loading cart…</p> }
    @if (!loading && cart.cartDetails.length === 0) {
      <section class="empty-state">
        <h2>Your cart is empty.</h2>
        <p class="muted">Find something you like and it will appear here.</p>
        <a class="primary-button" routerLink="/">Browse products</a>
      </section>
    } @else if (!loading) {
      <section class="cart-list">
        @for (item of cart.cartDetails; track item.cartDetailsId) {
          <article class="cart-row">
            <div>
              <h2>{{ item.product?.name || 'Product' }}</h2>
              <p class="muted">Quantity: {{ item.count }}</p>
            </div>
            <strong>{{ ((item.product?.price || 0) * item.count) | currency:'VND':'symbol':'1.0-0' }}</strong>
            <button class="danger-button" type="button" (click)="remove(item.cartDetailsId)">Remove</button>
          </article>
        }
        <div class="cart-total"><span>Total</span><strong>{{ cart.cartHeader.cartTotal | currency:'VND':'symbol':'1.0-0' }}</strong></div>
      </section>
    }
  `
})
export class CartComponent {
  private readonly api = inject(ApiService);
  protected cart: Cart = { cartHeader: { cartHeaderId: 0, discount: 0, cartTotal: 0 }, cartDetails: [] };
  protected loading = true;
  protected error = '';

  constructor() { this.load(); }

  remove(id: number): void {
    this.api.removeCartItem(id).subscribe({ next: () => this.load(), error: () => this.error = 'Unable to update your cart.' });
  }

  private load(): void {
    this.loading = true;
    this.api.cart().subscribe({
      next: cart => { this.cart = cart; this.loading = false; },
      error: () => { this.error = 'Sign in to view your cart.'; this.loading = false; }
    });
  }
}
