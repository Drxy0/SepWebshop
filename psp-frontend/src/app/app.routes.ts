import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Pay } from './components/pay/pay';
import { Index } from './components/index/index';
import { authGuard } from './guards/auth-guard';
import { PayCrypto } from './components/pay-crypto/pay-crypto';

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
    path: 'pay/crypto',
    component: PayCrypto,
  },
  {
    path: 'index',
    component: Index,
    canActivate: [authGuard],
  },
];
