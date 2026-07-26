import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { Cart, CartItem } from '../../core/models';
import { NotificationService } from '../../core/notification.service';

@Component({
  standalone: true,
  imports: [CurrencyPipe, ReactiveFormsModule, RouterLink],
  template: `
    <header class="page-heading">
      <p class="route-code">CART / SHOPPING CART SERVICE</p>
      <h1>Your dispatch</h1>
      <p>Review quantities and promotions before the next service takes ownership.</p>
    </header>

    @if (error) {
      <div class="inline-alert" role="alert">
        <strong>Cart route interrupted.</strong>
        <span>{{ error }}</span>
      </div>
    }

    @if (loading) {
      <section class="cart-layout" aria-label="Loading cart" aria-busy="true">
        <div class="cart-lines">
          @for (placeholder of [1, 2, 3]; track placeholder) {
            <div class="cart-line">
              <div class="skeleton cart-image"></div>
              <div>
                <div class="skeleton skeleton-line short"></div>
                <div class="skeleton skeleton-heading"></div>
              </div>
            </div>
          }
        </div>
      </section>
    } @else if (cart.cartDetails.length === 0) {
      <section class="state-panel state-panel--cart">
        <p class="route-code">CART / EMPTY</p>
        <h2>No items are waiting for dispatch.</h2>
        <p>Return to the catalogue and route a product here.</p>
        <a class="button button-primary" routerLink="/">Browse the catalogue</a>
      </section>
    } @else {
      <section class="cart-layout">
        <div class="cart-lines" aria-label="Cart items">
          @for (item of cart.cartDetails; track item.cartDetailsId) {
            <article class="cart-line">
              <a class="cart-image" [routerLink]="['/products', item.productId]">
                <img
                  [src]="item.product?.imageUrl || item.product?.imageLocalPath || '/images/product-placeholder.svg'"
                  [alt]="item.product?.name || 'Product'"
                />
              </a>
              <div class="cart-line-copy">
                <p class="route-code">ITEM / {{ paddedId(item.productId) }}</p>
                <h2><a [routerLink]="['/products', item.productId]">{{ item.product?.name || 'Product' }}</a></h2>
                <button class="text-action text-action--danger" type="button" (click)="remove(item)" [disabled]="pendingIds.has(item.cartDetailsId)">
                  Remove
                </button>
              </div>
              <div class="quantity-stepper" [attr.aria-label]="'Quantity for ' + (item.product?.name || 'product')">
                <button
                  type="button"
                  (click)="updateQuantity(item, item.count - 1)"
                  [disabled]="item.count <= 1 || pendingIds.has(item.cartDetailsId)"
                  aria-label="Decrease quantity"
                >−</button>
                <output aria-live="polite">{{ item.count }}</output>
                <button
                  type="button"
                  (click)="updateQuantity(item, item.count + 1)"
                  [disabled]="item.count >= 20 || pendingIds.has(item.cartDetailsId)"
                  aria-label="Increase quantity"
                >+</button>
              </div>
              <strong class="line-total">
                {{ ((item.product?.price || 0) * item.count) | currency:'VND':'symbol':'1.0-0' }}
              </strong>
            </article>
          }
        </div>

        <aside class="cart-summary" aria-labelledby="summary-title">
          <p class="route-code">ROUTE / SUMMARY</p>
          <h2 id="summary-title">Cart summary</h2>

          <dl>
            <div><dt>Subtotal</dt><dd>{{ subtotal | currency:'VND':'symbol':'1.0-0' }}</dd></div>
            @if (cart.cartHeader.discount > 0) {
              <div class="discount-line"><dt>Promotion</dt><dd>−{{ cart.cartHeader.discount | currency:'VND':'symbol':'1.0-0' }}</dd></div>
            }
            <div class="summary-total"><dt>Total</dt><dd>{{ cart.cartHeader.cartTotal | currency:'VND':'symbol':'1.0-0' }}</dd></div>
          </dl>

          @if (cart.cartHeader.couponCode) {
            <div class="active-coupon">
              <span><small>Promotion applied</small><strong>{{ cart.cartHeader.couponCode }}</strong></span>
              <button type="button" class="text-action" (click)="removeCoupon()" [disabled]="couponPending">Remove</button>
            </div>
          } @else {
            <form class="coupon-form" (ngSubmit)="applyCoupon()">
              <label for="coupon">Promotion code</label>
              <div>
                <input id="coupon" [formControl]="couponControl" placeholder="Enter code" autocomplete="off" />
                <button class="button button-secondary" type="submit" [disabled]="couponControl.invalid || couponPending">
                  {{ couponPending ? 'Checking…' : 'Apply' }}
                </button>
              </div>
            </form>
          }

          <button class="button button-primary button-full" type="button" (click)="emailCart()" [disabled]="emailPending">
            {{ emailPending ? 'Routing request…' : 'Email this cart' }}
          </button>
          <p class="summary-note">This action publishes a message for the Notification service. It does not place an order.</p>
        </aside>
      </section>
    }
  `
})
export class CartComponent {
  private readonly api = inject(ApiService);
  private readonly notifications = inject(NotificationService);
  protected readonly couponControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(32)]
  });
  protected cart: Cart = { cartHeader: { cartHeaderId: 0, discount: 0, cartTotal: 0 }, cartDetails: [] };
  protected readonly pendingIds = new Set<number>();
  protected loading = true;
  protected error = '';
  protected couponPending = false;
  protected emailPending = false;

  constructor() { this.load(); }

  get subtotal(): number {
    return this.cart.cartDetails.reduce(
      (total, item) => total + (item.product?.price ?? 0) * item.count,
      0
    );
  }

  updateQuantity(item: CartItem, count: number): void {
    if (count < 1 || count > 20 || this.pendingIds.has(item.cartDetailsId)) return;
    this.pendingIds.add(item.cartDetailsId);
    this.api.updateCartItem(item, count).pipe(
      finalize(() => this.pendingIds.delete(item.cartDetailsId))
    ).subscribe({
      next: response => response.isSuccess
        ? this.load(false)
        : this.notifications.show(response.message || 'Quantity could not be updated.', 'error'),
      error: () => this.notifications.show('Quantity could not be updated. Try again.', 'error')
    });
  }

  remove(item: CartItem): void {
    if (this.pendingIds.has(item.cartDetailsId)) return;
    this.pendingIds.add(item.cartDetailsId);
    this.api.removeCartItem(item.cartDetailsId).pipe(
      finalize(() => this.pendingIds.delete(item.cartDetailsId))
    ).subscribe({
      next: () => {
        this.notifications.show(`${item.product?.name || 'Item'} was removed.`, 'info');
        this.load(false);
      },
      error: () => this.notifications.show('The item could not be removed. Try again.', 'error')
    });
  }

  applyCoupon(): void {
    if (this.couponControl.invalid || this.couponPending) return;
    this.couponPending = true;
    const code = this.couponControl.value;
    this.api.applyCoupon(this.cart, code).pipe(
      finalize(() => this.couponPending = false)
    ).subscribe({
      next: response => {
        if (!response.isSuccess) {
          this.notifications.show(response.message || 'That promotion code is not valid.', 'error');
          return;
        }
        this.notifications.show(`${code.toUpperCase()} was applied.`, 'success');
        this.couponControl.setValue('');
        this.load(false);
      },
      error: () => this.notifications.show('The promotion service could not validate this code.', 'error')
    });
  }

  removeCoupon(): void {
    if (this.couponPending) return;
    this.couponPending = true;
    this.api.removeCoupon().pipe(
      finalize(() => this.couponPending = false)
    ).subscribe({
      next: () => {
        this.notifications.show('Promotion removed.', 'info');
        this.load(false);
      },
      error: () => this.notifications.show('The promotion could not be removed.', 'error')
    });
  }

  emailCart(): void {
    if (this.emailPending) return;
    this.emailPending = true;
    this.api.emailCart(this.cart).pipe(
      finalize(() => this.emailPending = false)
    ).subscribe({
      next: response => response.isSuccess
        ? this.notifications.show('Cart message accepted by the Notification route.', 'success')
        : this.notifications.show(response.message || 'The cart message was not accepted.', 'error'),
      error: () => this.notifications.show('The notification route is unavailable. Your cart is unchanged.', 'error')
    });
  }

  private load(showLoading = true): void {
    if (showLoading) this.loading = true;
    this.error = '';
    this.api.cart().subscribe({
      next: cart => { this.cart = cart; this.loading = false; },
      error: () => {
        this.error = 'The Cart service did not answer. Your existing cart has not been changed.';
        this.loading = false;
      }
    });
  }

  paddedId(id: number): string {
    return id.toString().padStart(4, '0');
  }
}
