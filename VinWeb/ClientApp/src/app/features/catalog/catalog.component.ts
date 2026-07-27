/*
THESIS: Vin Shop is a dispatch catalogue, not a generic rounded-card marketplace.
OWN-WORLD: Mineral ink, paper white, one electric routing accent, oversized product imagery, compact route codes.
STORY: Discover a focused selection, narrow it quickly, inspect one item, then route it to the cart with clear confirmation.
FIRST VIEWPORT: Full-bleed ink hero; VIN SHOP is the loudest mark, one featured product image owns the right plane, catalogue CTA sits low-left.
FORM: Parcel-routing catalogue, assigned grounded direction 4, seed 071a78d5.
*/
import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { Product } from '../../core/models';
import { NotificationService } from '../../core/notification.service';

type SortMode = 'featured' | 'price-asc' | 'price-desc' | 'name';

@Component({
  standalone: true,
  imports: [CurrencyPipe, ReactiveFormsModule, RouterLink],
  template: `
    <section class="catalogue-hero" aria-labelledby="catalogue-title">
      <div class="hero-copy">
        <p class="route-code route-code--light">VIN / SELECTED GOODS</p>
        <h1 id="catalogue-title"><span>VIN</span> SHOP</h1>
        <p class="hero-intro">Useful objects, selected with intent and routed without friction.</p>
        <a class="button button-accent" href="#catalogue">View the dispatch</a>
      </div>

      <div class="hero-visual" aria-hidden="true">
        @if (featuredProduct) {
          <img
            src="/assets/images/catalog-fallback.svg"
            [src]="featuredProduct.imageUrl || featuredProduct.imageLocalPath || '/assets/images/catalog-fallback.svg'"
            alt=""
          />
          <div class="hero-caption">
            <span>FEATURED / {{ paddedId(featuredProduct.productId) }}</span>
            <strong>{{ featuredProduct.name }}</strong>
          </div>
        } @else {
          <div class="hero-placeholder">VIN</div>
        }
      </div>
      <p class="hero-route" aria-hidden="true">AUTH → CATALOGUE → CART → DISPATCH</p>
    </section>

    <section id="catalogue" class="catalogue-section" aria-labelledby="dispatch-title">
      <header class="catalogue-heading">
        <div>
          <p class="route-code">LIVE / PRODUCT SERVICE</p>
          <h2 id="dispatch-title">Current dispatch</h2>
        </div>
        <p>{{ products.length }} items returned by the catalogue boundary.</p>
      </header>

      <div class="catalogue-tools" aria-label="Catalogue filters">
        <label class="search-field">
          <span class="sr-only">Search products</span>
          <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="m21 21-4.35-4.35m2.35-5.65a8 8 0 1 1-16 0 8 8 0 0 1 16 0Z"></path>
          </svg>
          <input [formControl]="searchControl" type="search" placeholder="Search the dispatch" />
        </label>

        <div class="filter-row">
          <label>
            <span class="sr-only">Filter by category</span>
            <select [value]="selectedCategory" (change)="setCategory($event)">
              <option value="all">All categories</option>
              @for (category of categories; track category) {
                <option [value]="category">{{ category }}</option>
              }
            </select>
          </label>
          <label>
            <span class="sr-only">Sort products</span>
            <select [value]="sortMode" (change)="setSort($event)">
              <option value="featured">Featured order</option>
              <option value="price-asc">Price: low to high</option>
              <option value="price-desc">Price: high to low</option>
              <option value="name">Name: A–Z</option>
            </select>
          </label>
        </div>
      </div>

      @if (loading) {
        <div class="product-grid" aria-label="Loading products" aria-busy="true">
          @for (placeholder of loadingPlaceholders; track placeholder) {
            <article class="product-tile product-tile--loading">
              <div class="skeleton skeleton-media"></div>
              <div class="skeleton skeleton-line short"></div>
              <div class="skeleton skeleton-heading"></div>
            </article>
          }
        </div>
      } @else if (error) {
        <section class="state-panel" role="alert">
          <p class="route-code">CATALOGUE / INTERRUPTED</p>
          <h3>The product route is unavailable.</h3>
          <p>{{ error }}</p>
          <button class="button button-secondary" type="button" (click)="loadProducts()">Try again</button>
        </section>
      } @else {
        <div class="results-meta" aria-live="polite">
          <span>{{ visibleProducts.length }} item{{ visibleProducts.length === 1 ? '' : 's' }}</span>
          @if (hasActiveFilters) {
            <button type="button" class="text-action" (click)="clearFilters()">Clear filters</button>
          }
        </div>

        <div class="product-grid" aria-label="Product catalogue">
          @for (product of visibleProducts; track product.productId; let index = $index) {
            <article class="product-tile" [class.product-tile--wide]="index === 0 && !hasActiveFilters">
              <a class="product-media" [routerLink]="['/products', product.productId]">
                <img
                  src="/assets/images/catalog-fallback.svg"
                  [src]="product.imageUrl || product.imageLocalPath || '/assets/images/catalog-fallback.svg'"
                  [alt]="product.name"
                  loading="lazy"
                />
                <span class="product-route">ITEM / {{ paddedId(product.productId) }}</span>
              </a>
              <div class="product-copy">
                <p>{{ product.categoryName || 'Vin selection' }}</p>
                <h3><a [routerLink]="['/products', product.productId]">{{ product.name }}</a></h3>
                <div class="product-action-row">
                  <strong>{{ product.price | currency:'VND':'symbol':'1.0-0' }}</strong>
                  <button
                    class="icon-action"
                    type="button"
                    (click)="add(product)"
                    [disabled]="addingIds.has(product.productId)"
                    [attr.aria-label]="'Add ' + product.name + ' to cart'"
                  >
                    <span aria-hidden="true">{{ addingIds.has(product.productId) ? '…' : '+' }}</span>
                  </button>
                </div>
              </div>
            </article>
          } @empty {
            <section class="state-panel state-panel--wide">
              <p class="route-code">NO MATCH / FILTERED</p>
              <h3>No item matches this route.</h3>
              <p>Clear the filters or try a broader product name.</p>
              <button class="button button-secondary" type="button" (click)="clearFilters()">Reset catalogue</button>
            </section>
          }
        </div>
      }
    </section>

    <section class="service-proof" aria-label="How the storefront works">
      <p class="route-code route-code--light">ONE ACTION / CLEAR BOUNDARIES</p>
      <h2>The storefront stays simple. The system behind it does not hide.</h2>
      <ol>
        <li><span>01</span><strong>Browse</strong><small>Product API owns the catalogue.</small></li>
        <li><span>02</span><strong>Route</strong><small>The BFF keeps internal services private.</small></li>
        <li><span>03</span><strong>Confirm</strong><small>Cart API owns the customer selection.</small></li>
      </ol>
    </section>
  `
})
export class CatalogComponent {
  private readonly api = inject(ApiService);
  private readonly notifications = inject(NotificationService);

  protected readonly searchControl = new FormControl('', { nonNullable: true });
  protected readonly loadingPlaceholders = [1, 2, 3, 4, 5, 6];
  protected readonly addingIds = new Set<number>();
  protected products: Product[] = [];
  protected loading = true;
  protected error = '';
  protected selectedCategory = 'all';
  protected sortMode: SortMode = 'featured';

  constructor() {
    this.loadProducts();
  }

  get featuredProduct(): Product | undefined {
    return this.products[0];
  }

  get categories(): string[] {
    return [...new Set(this.products.map(product => product.categoryName).filter(Boolean))]
      .sort((left, right) => left.localeCompare(right));
  }

  get hasActiveFilters(): boolean {
    return this.searchControl.value.trim().length > 0 ||
      this.selectedCategory !== 'all' ||
      this.sortMode !== 'featured';
  }

  get visibleProducts(): Product[] {
    const search = this.searchControl.value.trim().toLocaleLowerCase();
    const filtered = this.products.filter(product => {
      const matchesSearch = !search ||
        product.name.toLocaleLowerCase().includes(search) ||
        product.description.toLocaleLowerCase().includes(search);
      const matchesCategory = this.selectedCategory === 'all' ||
        product.categoryName === this.selectedCategory;
      return matchesSearch && matchesCategory;
    });

    return [...filtered].sort((left, right) => {
      if (this.sortMode === 'price-asc') return left.price - right.price;
      if (this.sortMode === 'price-desc') return right.price - left.price;
      if (this.sortMode === 'name') return left.name.localeCompare(right.name);
      return 0;
    });
  }

  loadProducts(): void {
    this.loading = true;
    this.error = '';
    this.api.products().subscribe({
      next: products => {
        this.products = products;
        this.loading = false;
      },
      error: () => {
        this.error = 'The Product service did not answer. Check the stack and try again.';
        this.loading = false;
      }
    });
  }

  add(product: Product): void {
    if (this.addingIds.has(product.productId)) return;
    this.addingIds.add(product.productId);
    this.api.addToCart(product.productId).pipe(
      finalize(() => this.addingIds.delete(product.productId))
    ).subscribe({
      next: response => response.isSuccess
        ? this.notifications.show(`${product.name} was added to your cart.`, 'success')
        : this.notifications.show(response.message || 'The cart could not be updated.', 'error'),
      error: () => this.notifications.show('Sign in to route this item to your cart.', 'error')
    });
  }

  clearFilters(): void {
    this.searchControl.setValue('');
    this.selectedCategory = 'all';
    this.sortMode = 'featured';
  }

  setCategory(event: Event): void {
    this.selectedCategory = (event.target as HTMLSelectElement).value;
  }

  setSort(event: Event): void {
    this.sortMode = (event.target as HTMLSelectElement).value as SortMode;
  }

  paddedId(id: number): string {
    return id.toString().padStart(4, '0');
  }
}
