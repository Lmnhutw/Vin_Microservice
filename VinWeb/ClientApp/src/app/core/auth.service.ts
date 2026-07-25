import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, tap } from 'rxjs';
import { ApiService } from './api.service';
import { User } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly userSubject = new BehaviorSubject<User | null>(null);
  readonly user$ = this.userSubject.asObservable();

  load(): Observable<User | null> {
    return this.api.me().pipe(
      map(response => response.result as User | null),
      tap(user => this.userSubject.next(user)),
      catchError(() => {
        this.userSubject.next(null);
        return of(null);
      })
    );
  }

  login(username: string, password: string): Observable<User> {
    return this.api.login(username, password).pipe(
      map(response => {
        if (!response.isSuccess || !response.result) {
          throw new Error(response.message || 'Unable to sign in.');
        }
        return response.result as User;
      }),
      tap(user => this.userSubject.next(user))
    );
  }

  register(email: string, fullName: string, phoneNumber: string, password: string): Observable<unknown> {
    return this.api.register(email, fullName, phoneNumber, password).pipe(
      map(response => {
        if (!response.isSuccess) throw new Error(response.message || 'Unable to register.');
        return response.result;
      })
    );
  }

  logout(): Observable<unknown> {
    return this.api.logout().pipe(tap(() => this.userSubject.next(null)));
  }
}
