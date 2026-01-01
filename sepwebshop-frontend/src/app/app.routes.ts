import { Routes } from '@angular/router';
import { authGuard } from './guards/auth-guard';
import { logInGuard } from './guards/log-in-guard';
import { Index } from './components/index';
import { Login } from './components/login/login';
import { Register } from './components/register/register';

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
];
