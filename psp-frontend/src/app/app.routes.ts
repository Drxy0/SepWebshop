import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Pay } from './components/pay/pay';
import { Index } from './components/index/index';
import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    component: Login,
  },
  {
    path: 'pay',
    component: Pay,
  },
  {
    path: 'index',
    component: Index,
    canActivate: [authGuard],
  },
];
