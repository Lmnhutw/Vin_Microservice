import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize, switchMap } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { Product } from '../../core/models';
import { NotificationService } from '../../core/notification.service';

@Component({
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  template: `
    <a class="back-link" routerLink="/">
      <span aria-hidden="true">←</span> Back to catalogue
    </a>

    @if (loading) {
      <section class="product-detail product-detail--loading" aria-label="Loading product">
        <div class="skeleton skeleton-media"></div>
        <div class="detail-copy">
          <div class="skeleton skeleton-line short"></div>
          <div class="skeleton skeleton-heading"></div>
          <div class="skeleton skeleton-line"></div>
          <div class="skeleton skeleton-line"></div>
        </div>
      </section>
    } @else if (error || !product) {
      <section class="state-panel" role="alert">
        <p class="route-code">ROUTE / PRODUCT</p>
        <h1>That product could not be routed.</h1>
        <p>{{ error }}</p>
        <a class="button button-primary" routerLink="/">Return to catalogue</a>
      </section>
    } @else {
      <article class="product-detail">
        <figure class="detail-media">
          <img
            src="/assets/images/catalog-fallback.svg"
            [src]="product.imageUrl || product.imageLocalPath || '/assets/images/catalog-fallback.svg'"
            [alt]="product.name"
          />
          <figcaption>{{ product.categoryName || 'Vin selection' }}</figcaption>
        </figure>
        <div class="detail-copy">
          <p class="route-code">ITEM / {{ paddedId(product.productId) }}</p>
          <h1>{{ product.name }}</h1>
          <p class="detail-price">{{ product.price | currency:'VND':'symbol':'1.0-0' }}</p>
          <p class="detail-description">{{ product.description }}</p>

          <div class="detail-actions">
            <div class="quantity-stepper" aria-label="Quantity">
              <button type="button" (click)="decrease()" [disabled]="quantity === 1" aria-label="Decrease quantity">−</button>
              <output aria-live="polite">{{ quantity }}</output>
              <button type="button" (click)="increase()" [disabled]="quantity === 20" aria-label="Increase quantity">+</button>
            </div>
            <button class="button button-primary" type="button" (click)="add()" [disabled]="adding">
              {{ adding ? 'Routing…' : 'Add to cart' }}
            </button>
          </div>
          <p class="detail-note">Cart updates are confirmed by the Cart service before this page reports success.</p>
        </div>
      </article>
    }
  `
})
export class ProductDetailComponent {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly notifications = inject(NotificationService);

  protected product?: Product;
  protected quantity = 1;
  protected loading = true;
  protected adding = false;
  protected error = '';

  constructor() {
    this.route.paramMap.pipe(
      switchMap(params => this.api.product(Number(params.get('id'))))
    ).subscribe({
      next: product => {
        this.product = product;
        this.loading = false;
      },
      error: () => {
        this.error = 'The catalogue service did not return this item. Try the catalogue again.';
        this.loading = false;
      }
    });
  }

  increase(): void {
    this.quantity = Math.min(20, this.quantity + 1);
  }

  decrease(): void {
    this.quantity = Math.max(1, this.quantity - 1);
  }

  add(): void {
    if (!this.product || this.adding) return;
    this.adding = true;
    this.api.addToCart(this.product.productId, this.quantity).pipe(
      finalize(() => this.adding = false)
    ).subscribe({
      next: response => response.isSuccess
        ? this.notifications.show(`${this.product?.name} was added to your cart.`, 'success')
        : this.notifications.show(response.message || 'The cart could not be updated.', 'error'),
      error: () => this.notifications.show('Sign in to route this item to your cart.', 'error')
    });
  }

  paddedId(id: number): string {
    return id.toString().padStart(4, '0');
  }
}
