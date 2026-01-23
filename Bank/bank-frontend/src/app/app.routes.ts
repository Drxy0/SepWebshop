import { Routes } from '@angular/router';
import { CardPayment } from './card-payment/card-payment';

export const routes: Routes = [
  {
    path: 'pay/card/:paymentRequestId',
    component: CardPayment
  }
];
