import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Cart, CartItem, Coupon, Order, Product, ResponseDto, User } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/bff';

  products(): Observable<Product[]> {
    return this.http.get<ResponseDto<Product[]>>(`${this.baseUrl}/products`).pipe(
      map(response => response.result ?? [])
    );
  }

  product(id: number): Observable<Product> {
    return this.http.get<ResponseDto<Product>>(`${this.baseUrl}/products/${id}`).pipe(
      map(response => {
        if (!response.result) throw new Error(response.message || 'Product not found.');
        return response.result;
      })
    );
  }

  login(username: string, password: string): Observable<ResponseDto<{ id: string; email: string; fullName: string }>> {
    return this.http.post<ResponseDto<{ id: string; email: string; fullName: string }>>(
      `${this.baseUrl}/auth/login`, { userName: username, password });
  }

  register(email: string, fullName: string, phoneNumber: string, password: string): Observable<ResponseDto<unknown>> {
    return this.http.post<ResponseDto<unknown>>(`${this.baseUrl}/auth/register`, {
      email, fullName, phoneNumber, password
    });
  }

  forgotPassword(email: string): Observable<ResponseDto<unknown>> {
    return this.http.post<ResponseDto<unknown>>(`${this.baseUrl}/auth/forgot-password`, { email });
  }

  resetPassword(email: string, token: string, newPassword: string): Observable<ResponseDto<unknown>> {
    return this.http.post<ResponseDto<unknown>>(`${this.baseUrl}/auth/reset-password`, {
      email, token, newPassword
    });
  }

  logout(): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/auth/logout`, {});
  }

  me(): Observable<ResponseDto<User>> {
    return this.http.get<ResponseDto<User>>(`${this.baseUrl}/auth/me`);
  }

  cart(): Observable<Cart> {
    return this.http.get<ResponseDto<Cart>>(`${this.baseUrl}/cart`).pipe(
      map(response => response.result ?? { cartHeader: { cartHeaderId: 0, discount: 0, cartTotal: 0 }, cartDetails: [] })
    );
  }

  addToCart(productId: number, count = 1): Observable<ResponseDto<Cart>> {
    return this.http.post<ResponseDto<Cart>>(`${this.baseUrl}/cart`, {
      cartHeader: {},
      cartDetails: [{ productId, count }]
    });
  }

  updateCartItem(item: CartItem, count: number): Observable<ResponseDto<Cart>> {
    return this.http.post<ResponseDto<Cart>>(`${this.baseUrl}/cart`, {
      cartHeader: {},
      cartDetails: [{
        cartDetailsId: item.cartDetailsId,
        productId: item.productId,
        count
      }]
    });
  }

  removeCartItem(id: number): Observable<unknown> {
    return this.http.delete(`${this.baseUrl}/cart/items/${id}`);
  }

  applyCoupon(cart: Cart, couponCode: string): Observable<ResponseDto<Cart>> {
    return this.http.post<ResponseDto<Cart>>(`${this.baseUrl}/cart/coupon`, {
      ...cart,
      cartHeader: { ...cart.cartHeader, couponCode: couponCode.trim() }
    });
  }

  removeCoupon(): Observable<ResponseDto<Cart>> {
    return this.http.delete<ResponseDto<Cart>>(`${this.baseUrl}/cart/coupon`);
  }

  emailCart(cart: Cart): Observable<ResponseDto<unknown>> {
    return this.http.post<ResponseDto<unknown>>(`${this.baseUrl}/cart/email`, cart);
  }

  checkout(): Observable<ResponseDto<Order>> {
    return this.http.post<ResponseDto<Order>>(`${this.baseUrl}/checkout`, {});
  }

  order(orderId: string): Observable<Order> {
    return this.http.get<ResponseDto<Order>>(`${this.baseUrl}/orders/${orderId}`).pipe(
      map(response => {
        if (!response.result) throw new Error(response.message || 'Order not found.');
        return response.result;
      })
    );
  }

  coupons(): Observable<Coupon[]> {
    return this.http.get<ResponseDto<Coupon[]>>(`${this.baseUrl}/admin/coupons`).pipe(
      map(response => response.result ?? [])
    );
  }

  createProduct(product: Omit<Product, 'productId'>): Observable<ResponseDto<Product>> {
    return this.http.post<ResponseDto<Product>>(`${this.baseUrl}/admin/products`, product);
  }

  updateProduct(product: Product): Observable<ResponseDto<Product>> {
    return this.http.put<ResponseDto<Product>>(
      `${this.baseUrl}/admin/products/${product.productId}`,
      product
    );
  }

  deleteProduct(id: number): Observable<ResponseDto<unknown>> {
    return this.http.delete<ResponseDto<unknown>>(`${this.baseUrl}/admin/products/${id}`);
  }

  createCoupon(coupon: Omit<Coupon, 'couponId'>): Observable<ResponseDto<Coupon>> {
    return this.http.post<ResponseDto<Coupon>>(`${this.baseUrl}/admin/coupons`, coupon);
  }

  updateCoupon(coupon: Coupon): Observable<ResponseDto<Coupon>> {
    return this.http.put<ResponseDto<Coupon>>(
      `${this.baseUrl}/admin/coupons/${coupon.couponId}`,
      coupon
    );
  }

  deleteCoupon(id: number): Observable<ResponseDto<unknown>> {
    return this.http.delete<ResponseDto<unknown>>(`${this.baseUrl}/admin/coupons/${id}`);
  }
}
