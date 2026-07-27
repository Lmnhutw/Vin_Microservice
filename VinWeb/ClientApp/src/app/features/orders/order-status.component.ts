import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subscription, timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ApiService } from '../../core/api.service';
import { Order, OrderStatus } from '../../core/models';

@Component({
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink],
  template: `
    <header class="page-heading">
      <p class="route-code">ORDER / SAGA TRACKING</p>
      <h1>Dispatch in motion</h1>
      <p>This screen exposes eventual consistency: order creation returns first, then Payment and Inventory finish in separate services.</p>
    </header>

    @if (error) {
      <section class="state-panel"><p class="route-code">ORDER / INTERRUPTED</p><h2>We could not retrieve this order.</h2><p>{{ error }}</p><a class="button button-primary" routerLink="/cart">Return to cart</a></section>
    } @else if (!order) {
      <section class="state-panel" aria-busy="true"><p class="route-code">ORDER / LOADING</p><h2>Tracing the order route…</h2><div class="skeleton skeleton-heading"></div></section>
    } @else {
      <section class="order-tracker" [class.order-tracker--terminal]="isTerminal(order.status)">
        <div class="order-tracker__heading">
          <div><p class="route-code">{{ order.orderNumber }}</p><h2>{{ statusLabel(order.status) }}</h2></div>
          <strong>{{ order.totalAmount | currency:'VND':'symbol':'1.0-0' }}</strong>
        </div>
        <p class="order-tracker__summary">Created {{ order.createdOnUtc | date:'medium' }}. {{ statusCopy(order.status) }}</p>
        @if (order.failureReason) { <div class="inline-alert" role="alert"><strong>Workflow note.</strong><span>{{ order.failureReason }}</span></div> }

        <ol class="saga-timeline" aria-label="Order saga steps">
          <li [class.is-complete]="stepComplete('order')"><span>01</span><div><strong>Order owns the saga</strong><small>Order and its outbox are committed together.</small></div></li>
          <li [class.is-complete]="stepComplete('payment')" [class.is-current]="order.status === 'PendingPayment'"><span>02</span><div><strong>Payment simulator</strong><small>Authorizes or rejects deterministically.</small></div></li>
          <li [class.is-complete]="stepComplete('inventory')" [class.is-current]="order.status === 'PendingInventory'"><span>03</span><div><strong>Inventory reservation</strong><small>Owns stock and can trigger compensation.</small></div></li>
          <li [class.is-complete]="isTerminal(order.status)" [class.is-current]="order.status === 'CompensatingPayment'"><span>04</span><div><strong>Final state</strong><small>Confirmed, rejected, or refund compensation completes.</small></div></li>
        </ol>
        <div class="order-lines">
          @for (line of order.lines; track line.productId) { <div><span>{{ line.quantity }} × {{ line.productName }}</span><strong>{{ (line.unitPrice * line.quantity) | currency:'VND':'symbol':'1.0-0' }}</strong></div> }
        </div>
        @if (!isTerminal(order.status)) { <p class="summary-note">Refreshing every 2.5 seconds while the distributed workflow is active.</p> }
      </section>
    }
  `
})
export class OrderStatusComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApiService);
  private polling?: Subscription;
  protected order?: Order;
  protected error = '';

  ngOnInit(): void {
    const orderId = this.route.snapshot.paramMap.get('id');
    if (!orderId) { this.error = 'Order id is missing.'; return; }
    this.polling = timer(0, 2500).pipe(switchMap(() => this.api.order(orderId))).subscribe({
      next: order => {
        this.order = order;
        if (this.isTerminal(order.status)) this.polling?.unsubscribe();
      },
      error: () => {
        this.error = 'The order service did not answer. It may still be processing; try opening this route again.';
        this.polling?.unsubscribe();
      }
    });
  }

  ngOnDestroy(): void { this.polling?.unsubscribe(); }

  protected isTerminal(status: OrderStatus): boolean { return status === 'Confirmed' || status === 'Rejected'; }

  protected stepComplete(step: 'order' | 'payment' | 'inventory'): boolean {
    if (!this.order) return false;
    if (step === 'order') return true;
    if (step === 'payment') return this.order.status !== 'PendingPayment';
    return this.order.status === 'Confirmed' || this.order.status === 'CompensatingPayment' || this.order.status === 'Rejected';
  }

  protected statusLabel(status: OrderStatus): string {
    return ({ PendingPayment: 'Awaiting payment', PendingInventory: 'Payment accepted', CompensatingPayment: 'Refunding payment', Confirmed: 'Order confirmed', Rejected: 'Order rejected' })[status];
  }

  protected statusCopy(status: OrderStatus): string {
    return ({ PendingPayment: 'Payment has not yet consumed the order event.', PendingInventory: 'Payment authorized the order; Inventory now owns the next decision.', CompensatingPayment: 'Inventory rejected the reservation, so the saga requested a refund.', Confirmed: 'Payment and inventory steps both completed.', Rejected: 'The workflow reached a terminal rejected state.' })[status];
  }
}
