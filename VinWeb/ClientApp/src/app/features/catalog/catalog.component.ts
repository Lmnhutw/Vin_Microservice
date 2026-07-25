import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { Product } from '../../core/models';

@Component({
  standalone: true,
  imports: [CurrencyPipe],
  template: `
    <section class="hero">
      <p class="eyebrow">Curated essentials</p>
      <h1>Good products, simply chosen.</h1>
      <p>Discover the latest products from Vin Shop.</p>
    </section>
    @if (loading) { <p class="muted" aria-live="polite">Loading products…</p> }
    @if (error) { <p class="error" role="alert">{{ error }}</p> }
    <section class="product-grid" aria-label="Product catalog">
      @for (product of products; track product.productId) {
        <article class="product-card">
          <img [src]="product.imageUrl || '/images/product-placeholder.svg'" [alt]="product.name" loading="lazy" />
          <div class="product-content">
            <p class="eyebrow">{{ product.categoryName }}</p>
            <h2>{{ product.name }}</h2>
            <p class="muted description">{{ product.description }}</p>
            <div class="product-footer">
              <strong>{{ product.price | currency:'VND':'symbol':'1.0-0' }}</strong>
              <button class="primary-button compact" type="button" (click)="add(product.productId)">Add to cart</button>
            </div>
          </div>
        </article>
      } @empty {
        @if (!loading) { <p class="muted">No products are available yet.</p> }
      }
    </section>
  `
})
export class CatalogComponent {
  private readonly api = inject(ApiService);
  protected products: Product[] = [];
  protected loading = true;
  protected error = '';

  constructor() {
    this.api.products().subscribe({
      next: products => { this.products = products; this.loading = false; },
      error: () => { this.error = 'The catalog is temporarily unavailable.'; this.loading = false; }
    });
  }

  add(productId: number): void {
    this.api.addToCart(productId).subscribe({
      next: () => this.error = '',
      error: () => this.error = 'Sign in before adding products to your cart.'
    });
  }
}
