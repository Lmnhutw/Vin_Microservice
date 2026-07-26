import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { ApiService } from './api.service';

export const adminGuard: CanActivateFn = () => {
  const api = inject(ApiService);
  const router = inject(Router);

  return api.me().pipe(
    map(response => response.result?.roles?.includes('ADMIN')
      ? true
      : router.createUrlTree(['/'])),
    catchError(() => of(router.createUrlTree(['/login'])))
  );
};
