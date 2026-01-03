import { Routes } from '@angular/router';
import { authGuard } from './guards/auth-guard';
import { logInGuard } from './guards/log-in-guard';
import { adminGuard } from './guards/admin-guard';
import { Index } from './components/index';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { CarsComponent } from './components/admin/cars/cars';
import { Insurances } from './components/admin/insurances/insurances';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    component: Login,
    canActivate: [logInGuard],
  },
  {
    path: 'index',
    component: Index,
    canActivate: [authGuard],
  },
  {
    path: 'register',
    component: Register,
    canActivate: [logInGuard],
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
];
