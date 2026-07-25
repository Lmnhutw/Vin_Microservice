import { Routes } from '@angular/router';
import { CatalogComponent } from './features/catalog/catalog.component';
import { CartComponent } from './features/cart/cart.component';
import { LoginComponent } from './features/auth/login.component';
import { RegisterComponent } from './features/auth/register.component';
import { ResetPasswordComponent } from './features/auth/reset-password.component';

export const routes: Routes = [
  { path: '', component: CatalogComponent, title: 'Vin Shop' },
  { path: 'login', component: LoginComponent, title: 'Sign in | Vin Shop' },
  { path: 'register', component: RegisterComponent, title: 'Create account | Vin Shop' },
  { path: 'reset-password', component: ResetPasswordComponent, title: 'Reset password | Vin Shop' },
  { path: 'cart', component: CartComponent, title: 'Cart | Vin Shop' },
  { path: '**', redirectTo: '' }
];
