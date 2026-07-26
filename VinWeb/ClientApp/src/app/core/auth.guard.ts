import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { ApiService } from './api.service';

export const authGuard: CanActivateFn = (_, state) => {
  const api = inject(ApiService);
  const router = inject(Router);

  return api.me().pipe(
    map(response => response.isSuccess && response.result
      ? true
      : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } })),
    catchError(() => of(
      router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } })
    ))
  );
};
