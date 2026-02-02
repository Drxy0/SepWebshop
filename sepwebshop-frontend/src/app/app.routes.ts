import { Routes } from '@angular/router';
import { authGuard } from './guards/auth-guard';
import { logInGuard } from './guards/log-in-guard';
import { adminGuard } from './guards/admin-guard';
import { Index } from './components/index';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { CarsComponent } from './components/admin/cars/cars';
import { Insurances } from './components/admin/insurances/insurances';
import { Success } from './components/success/success';
import { Error } from './components/error/error';
import { Failed } from './components/failed/failed';
import { MyOrders } from './components/my-orders/my-orders';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    component: Login,
    canActivate: [],
  },
  {
    path: 'index',
    component: Index,
    canActivate: [authGuard],
  },
  {
    path: 'register',
    component: Register,
    canActivate: [],
  },
  {
    path: 'admin/cars',
    component: CarsComponent,
    canActivate: [authGuard, adminGuard],
  },
  {
    path: 'admin/insurances',
    component: Insurances,
    canActivate: [authGuard, adminGuard],
  },
  {
    path: 'success',
    component: Success,
  },
  {
    path: 'error',
    component: Error,
  },
  {
    path: 'failed',
    component: Failed,
  },
  {
    path: 'my-orders',
    component: MyOrders,
    canActivate: [authGuard],
  },
];
