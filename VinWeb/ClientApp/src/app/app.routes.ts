import { Routes } from '@angular/router';
import { CatalogComponent } from './features/catalog/catalog.component';
import { CartComponent } from './features/cart/cart.component';
import { LoginComponent } from './features/auth/login.component';
import { RegisterComponent } from './features/auth/register.component';
import { ResetPasswordComponent } from './features/auth/reset-password.component';
import { ProductDetailComponent } from './features/catalog/product-detail.component';
import { authGuard } from './core/auth.guard';
import { adminGuard } from './core/admin.guard';
import { AdminComponent } from './features/admin/admin.component';
import { OrderStatusComponent } from './features/orders/order-status.component';

export const routes: Routes = [
  { path: '', component: CatalogComponent, title: 'Vin Shop' },
  { path: 'products/:id', component: ProductDetailComponent, title: 'Product | Vin Shop' },
  { path: 'login', component: LoginComponent, title: 'Sign in | Vin Shop' },
  { path: 'register', component: RegisterComponent, title: 'Create account | Vin Shop' },
  { path: 'reset-password', component: ResetPasswordComponent, title: 'Reset password | Vin Shop' },
  { path: 'cart', component: CartComponent, canActivate: [authGuard], title: 'Cart | Vin Shop' },
  { path: 'orders/:id', component: OrderStatusComponent, canActivate: [authGuard], title: 'Order tracking | Vin Shop' },
  { path: 'admin', component: AdminComponent, canActivate: [adminGuard], title: 'Admin | Vin Shop' },
  { path: '**', redirectTo: '' }
];
