import { Routes } from '@angular/router';
import { CardPayment } from './card-payment/card-payment';
import { QrPayment } from './qr-payment/qr-payment';

export const routes: Routes = [
  {
    path: 'pay/card/:paymentRequestId',
    component: CardPayment
  },
  {
    path: 'pay/qr/:paymentRequestId',
    component: QrPayment
  }
];
