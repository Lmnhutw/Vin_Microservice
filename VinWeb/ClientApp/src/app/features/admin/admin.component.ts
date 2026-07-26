import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { Coupon, Product } from '../../core/models';
import { NotificationService } from '../../core/notification.service';

type AdminSection = 'products' | 'coupons';
type DeletePrompt = { kind: AdminSection; id: number; name: string };

@Component({
  standalone: true,
  imports: [CurrencyPipe, ReactiveFormsModule],
  template: `
    <header class="admin-heading">
      <div>
        <p class="route-code">ADMIN / CONTENT OWNERSHIP</p>
        <h1>Commerce content</h1>
        <p>Write operations stay behind the BFF and are enforced again by the owning API.</p>
      </div>
      <div class="admin-status"><span>ROLE</span><strong>ADMIN</strong></div>
    </header>

    <main class="admin-workspace">
      <nav class="admin-tabs" aria-label="Admin sections">
        <button type="button" [class.is-active]="section === 'products'" [attr.aria-current]="section === 'products' ? 'page' : null" (click)="section = 'products'">
          Products <span>{{ products.length }}</span>
        </button>
        <button type="button" [class.is-active]="section === 'coupons'" [attr.aria-current]="section === 'coupons' ? 'page' : null" (click)="section = 'coupons'">
          Coupons <span>{{ coupons.length }}</span>
        </button>
      </nav>

      @if (loading) {
        <div class="admin-loading" aria-busy="true">
          <div class="skeleton skeleton-line short"></div>
          <div class="skeleton skeleton-heading"></div>
          <div class="skeleton skeleton-line"></div>
          <div class="skeleton skeleton-line"></div>
        </div>
      } @else if (error) {
        <section class="state-panel" role="alert">
          <p class="route-code">ADMIN / READ INTERRUPTED</p>
          <h2>Content could not be loaded.</h2>
          <p>{{ error }}</p>
          <button class="button button-secondary" type="button" (click)="load()">Try again</button>
        </section>
      } @else if (section === 'products') {
        <section class="admin-section" aria-labelledby="product-admin-title">
          <div class="admin-list">
            <header>
              <div><p class="route-code">PRODUCT API / OWNED DATA</p><h2 id="product-admin-title">Products</h2></div>
              <button class="button button-primary" type="button" (click)="newProduct()">New product</button>
            </header>
            <div class="admin-table-wrap">
              <table>
                <thead><tr><th>Product</th><th>Category</th><th>Price</th><th><span class="sr-only">Actions</span></th></tr></thead>
                <tbody>
                  @for (product of products; track product.productId) {
                    <tr>
                      <td>
                        <div class="admin-product-cell">
                          <img [src]="product.imageUrl || product.imageLocalPath || '/images/product-placeholder.svg'" alt="" />
                          <span><strong>{{ product.name }}</strong><small>ITEM / {{ paddedId(product.productId) }}</small></span>
                        </div>
                      </td>
                      <td>{{ product.categoryName }}</td>
                      <td>{{ product.price | currency:'VND':'symbol':'1.0-0' }}</td>
                      <td class="admin-actions">
                        <button type="button" (click)="editProduct(product)">Edit</button>
                        <button type="button" class="danger" (click)="requestDelete('products', product.productId, product.name)">Delete</button>
                      </td>
                    </tr>
                  } @empty {
                    <tr><td colspan="4">No products are owned by this service yet.</td></tr>
                  }
                </tbody>
              </table>
            </div>
          </div>

          <aside class="admin-editor" aria-labelledby="product-form-title">
            <p class="route-code">{{ productForm.controls.productId.value ? 'EDIT' : 'CREATE' }} / PRODUCT</p>
            <h2 id="product-form-title">{{ productForm.controls.productId.value ? 'Edit product' : 'New product' }}</h2>
            <form [formGroup]="productForm" (ngSubmit)="saveProduct()" novalidate>
              <label for="product-name">Name</label>
              <input id="product-name" formControlName="name" />
              <label for="product-category">Category</label>
              <input id="product-category" formControlName="categoryName" />
              <label for="product-price">Price (VND)</label>
              <input id="product-price" type="number" min="0" step="1000" formControlName="price" />
              <label for="product-description">Description</label>
              <textarea id="product-description" rows="5" formControlName="description"></textarea>
              <label for="product-image">Image URL</label>
              <input id="product-image" type="url" formControlName="imageUrl" />
              @if (productForm.invalid && productForm.touched) {
                <p class="error">Complete the name, category, description, and non-negative price.</p>
              }
              <div class="editor-actions">
                @if (productForm.controls.productId.value) {
                  <button class="button button-secondary" type="button" (click)="newProduct()">Cancel</button>
                }
                <button class="button button-primary" type="submit" [disabled]="productForm.invalid || saving">
                  {{ saving ? 'Saving…' : 'Save product' }}
                </button>
              </div>
            </form>
          </aside>
        </section>
      } @else {
        <section class="admin-section" aria-labelledby="coupon-admin-title">
          <div class="admin-list">
            <header>
              <div><p class="route-code">COUPON API / OWNED DATA</p><h2 id="coupon-admin-title">Coupons</h2></div>
              <button class="button button-primary" type="button" (click)="newCoupon()">New coupon</button>
            </header>
            <div class="admin-table-wrap">
              <table>
                <thead><tr><th>Code</th><th>Discount</th><th>Minimum</th><th><span class="sr-only">Actions</span></th></tr></thead>
                <tbody>
                  @for (coupon of coupons; track coupon.couponId) {
                    <tr>
                      <td><strong>{{ coupon.couponCode }}</strong><small class="table-code">COUPON / {{ paddedId(coupon.couponId) }}</small></td>
                      <td>{{ coupon.discountAmount | currency:'VND':'symbol':'1.0-0' }}</td>
                      <td>{{ coupon.minAmount | currency:'VND':'symbol':'1.0-0' }}</td>
                      <td class="admin-actions">
                        <button type="button" (click)="editCoupon(coupon)">Edit</button>
                        <button type="button" class="danger" (click)="requestDelete('coupons', coupon.couponId, coupon.couponCode)">Delete</button>
                      </td>
                    </tr>
                  } @empty {
                    <tr><td colspan="4">No coupons are owned by this service yet.</td></tr>
                  }
                </tbody>
              </table>
            </div>
          </div>

          <aside class="admin-editor" aria-labelledby="coupon-form-title">
            <p class="route-code">{{ couponForm.controls.couponId.value ? 'EDIT' : 'CREATE' }} / COUPON</p>
            <h2 id="coupon-form-title">{{ couponForm.controls.couponId.value ? 'Edit coupon' : 'New coupon' }}</h2>
            <form [formGroup]="couponForm" (ngSubmit)="saveCoupon()" novalidate>
              <label for="coupon-code">Code</label>
              <input id="coupon-code" formControlName="couponCode" />
              <label for="coupon-discount">Discount amount (VND)</label>
              <input id="coupon-discount" type="number" min="0" step="1000" formControlName="discountAmount" />
              <label for="coupon-minimum">Minimum cart amount (VND)</label>
              <input id="coupon-minimum" type="number" min="0" step="1000" formControlName="minAmount" />
              @if (couponForm.invalid && couponForm.touched) {
                <p class="error">Enter a code and non-negative discount and minimum amounts.</p>
              }
              <div class="editor-actions">
                @if (couponForm.controls.couponId.value) {
                  <button class="button button-secondary" type="button" (click)="newCoupon()">Cancel</button>
                }
                <button class="button button-primary" type="submit" [disabled]="couponForm.invalid || saving">
                  {{ saving ? 'Saving…' : 'Save coupon' }}
                </button>
              </div>
            </form>
          </aside>
        </section>
      }
    </main>

    @if (deletePrompt; as prompt) {
      <section class="delete-confirmation" role="alertdialog" aria-modal="true" aria-labelledby="delete-title">
        <div>
          <p class="route-code">DESTRUCTIVE / CONFIRM</p>
          <h2 id="delete-title">Delete {{ prompt.name }}?</h2>
          <p>The owning service will remove this record. This learning project does not provide undo.</p>
          <div>
            <button class="button button-secondary" type="button" (click)="deletePrompt = null">Keep it</button>
            <button class="button button-danger" type="button" (click)="confirmDelete()" [disabled]="deleting">
              {{ deleting ? 'Deleting…' : 'Delete permanently' }}
            </button>
          </div>
        </div>
      </section>
    }
  `
})
export class AdminComponent {
  private readonly api = inject(ApiService);
  private readonly fb = inject(FormBuilder);
  private readonly notifications = inject(NotificationService);

  protected section: AdminSection = 'products';
  protected products: Product[] = [];
  protected coupons: Coupon[] = [];
  protected loading = true;
  protected saving = false;
  protected deleting = false;
  protected error = '';
  protected deletePrompt: DeletePrompt | null = null;

  protected readonly productForm = this.fb.nonNullable.group({
    productId: 0,
    name: ['', Validators.required],
    categoryName: ['', Validators.required],
    price: [0, [Validators.required, Validators.min(0)]],
    description: ['', Validators.required],
    imageUrl: ''
  });

  protected readonly couponForm = this.fb.nonNullable.group({
    couponId: 0,
    couponCode: ['', Validators.required],
    discountAmount: [0, [Validators.required, Validators.min(0)]],
    minAmount: [0, [Validators.required, Validators.min(0)]]
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    let pending = 2;
    const complete = () => {
      pending -= 1;
      if (pending === 0) this.loading = false;
    };
    this.api.products().subscribe({
      next: products => { this.products = products; complete(); },
      error: () => { this.error = 'Product content could not be loaded.'; complete(); }
    });
    this.api.coupons().subscribe({
      next: coupons => { this.coupons = coupons; complete(); },
      error: () => { this.error = 'Coupon content could not be loaded or your admin session expired.'; complete(); }
    });
  }

  newProduct(): void {
    this.productForm.reset({ productId: 0, name: '', categoryName: '', price: 0, description: '', imageUrl: '' });
  }

  editProduct(product: Product): void {
    this.section = 'products';
    this.productForm.setValue({
      productId: product.productId,
      name: product.name,
      categoryName: product.categoryName,
      price: product.price,
      description: product.description,
      imageUrl: product.imageUrl || ''
    });
  }

  saveProduct(): void {
    if (this.productForm.invalid || this.saving) {
      this.productForm.markAllAsTouched();
      return;
    }
    this.saving = true;
    const value = this.productForm.getRawValue();
    const request = value.productId
      ? this.api.updateProduct(value)
      : this.api.createProduct({
          name: value.name,
          categoryName: value.categoryName,
          price: value.price,
          description: value.description,
          imageUrl: value.imageUrl
        });
    request.pipe(finalize(() => this.saving = false)).subscribe({
      next: response => {
        if (!response.isSuccess) {
          this.notifications.show(response.message || 'Product could not be saved.', 'error');
          return;
        }
        this.notifications.show('Product saved by Product API.', 'success');
        this.newProduct();
        this.load();
      },
      error: () => this.notifications.show('Product could not be saved. Check your admin session.', 'error')
    });
  }

  newCoupon(): void {
    this.couponForm.reset({ couponId: 0, couponCode: '', discountAmount: 0, minAmount: 0 });
  }

  editCoupon(coupon: Coupon): void {
    this.section = 'coupons';
    this.couponForm.setValue(coupon);
  }

  saveCoupon(): void {
    if (this.couponForm.invalid || this.saving) {
      this.couponForm.markAllAsTouched();
      return;
    }
    this.saving = true;
    const value = this.couponForm.getRawValue();
    const request = value.couponId
      ? this.api.updateCoupon(value)
      : this.api.createCoupon({
          couponCode: value.couponCode,
          discountAmount: value.discountAmount,
          minAmount: value.minAmount
        });
    request.pipe(finalize(() => this.saving = false)).subscribe({
      next: response => {
        if (!response.isSuccess) {
          this.notifications.show(response.message || 'Coupon could not be saved.', 'error');
          return;
        }
        this.notifications.show('Coupon saved by Coupon API.', 'success');
        this.newCoupon();
        this.load();
      },
      error: () => this.notifications.show('Coupon could not be saved. Check your admin session.', 'error')
    });
  }

  requestDelete(kind: AdminSection, id: number, name: string): void {
    this.deletePrompt = { kind, id, name };
  }

  confirmDelete(): void {
    if (!this.deletePrompt || this.deleting) return;
    this.deleting = true;
    const prompt = this.deletePrompt;
    const request = prompt.kind === 'products'
      ? this.api.deleteProduct(prompt.id)
      : this.api.deleteCoupon(prompt.id);
    request.pipe(finalize(() => this.deleting = false)).subscribe({
      next: response => {
        if (!response.isSuccess) {
          this.notifications.show(response.message || `${prompt.name} could not be deleted.`, 'error');
          return;
        }
        this.notifications.show(`${prompt.name} was deleted.`, 'info');
        this.deletePrompt = null;
        this.load();
      },
      error: () => this.notifications.show(`${prompt.name} could not be deleted.`, 'error')
    });
  }

  paddedId(id: number): string {
    return id.toString().padStart(4, '0');
  }
}
