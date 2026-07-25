import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Cart, Product, ResponseDto } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/bff';

  products(): Observable<Product[]> {
    return this.http.get<ResponseDto<Product[]>>(`${this.baseUrl}/products`).pipe(
      map(response => response.result ?? [])
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

  me(): Observable<ResponseDto<unknown>> {
    return this.http.get<ResponseDto<unknown>>(`${this.baseUrl}/auth/me`);
  }

  cart(): Observable<Cart> {
    return this.http.get<ResponseDto<Cart>>(`${this.baseUrl}/cart`).pipe(
      map(response => response.result ?? { cartHeader: { cartHeaderId: 0, discount: 0, cartTotal: 0 }, cartDetails: [] })
    );
  }

  addToCart(productId: number): Observable<ResponseDto<Cart>> {
    return this.http.post<ResponseDto<Cart>>(`${this.baseUrl}/cart`, {
      cartHeader: {},
      cartDetails: [{ productId, count: 1 }]
    });
  }

  removeCartItem(id: number): Observable<unknown> {
    return this.http.delete(`${this.baseUrl}/cart/items/${id}`);
  }
}
