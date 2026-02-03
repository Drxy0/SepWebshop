import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { User } from '../../services/user/user';
import { CommonModule } from '@angular/common';
import { PaymentService } from '../../services/payment/payment-service';
import { finalize } from 'rxjs';
import { CryptoPaymentResponse } from '../../models/interfaces/payment';

@Component({
  selector: 'app-pay',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pay.html',
})
export class Pay implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private userService = inject(User);
  private paymentService = inject(PaymentService);

  orderId = signal<string | null>(null);
  merchantId = signal<string | null>(null);
  availableMethods = signal<string[]>([]);

  isLoadingMethods = signal<boolean>(false);
  isProcessingPayment = signal<boolean>(false);
  selectedMethod = signal<string | null>(null);

  ngOnInit() {
    const params = this.route.snapshot.queryParamMap;
    this.orderId.set(params.get('orderId'));
    const mId = params.get('merchantId');
    this.merchantId.set(mId);

    if (mId) {
      this.loadMerchantMethods(mId);
    }
  }

  loadMerchantMethods(id: string) {
    this.isLoadingMethods.set(true);
    this.userService
      .getActiveMethods(id)
      .pipe(finalize(() => this.isLoadingMethods.set(false)))
      .subscribe({
        next: (methods) => {
          this.availableMethods.set(methods);
        },
        error: (err) => {
          console.error('Error fetching merchant methods:', err);
        },
      });
  }

  onPay(method: string) {
    const orderId = this.orderId();
    if (this.isProcessingPayment()) return;

    this.selectedMethod.set(method);

    if (!orderId) {
      return;
    }

    if (method.toLowerCase() === 'crypto') {
      this.isProcessingPayment.set(true);

      this.paymentService.initializeCryptoPayment(orderId).subscribe({
        next: (response: CryptoPaymentResponse) => {
          const dataUrl = `data:image/png;base64,${response.qrCodeBase64}`;

          this.router.navigate(['/pay/crypto'], {
            state: {
              qrUrl: dataUrl,
              orderId: response.merchantOrderId,
            },
          });
        },
        error: (err) => {
          console.error('Error initializing Crypto payment:', err);
          this.isProcessingPayment.set(false);
          this.selectedMethod.set(null);
        },
      });
    } else if (method.toLowerCase() === 'paypal') {
      this.isProcessingPayment.set(true);

      this.paymentService.initializePayPalPayment(orderId).subscribe({
        next: (response) => {
          window.location.href = response.approvalUrl;
        },
        error: (err) => {
          console.error('Error initializing PayPal payment:', err);
          this.isProcessingPayment.set(false);
          this.selectedMethod.set(null);
        },
      });
    } else if (method.toLowerCase() === 'qr') {
      this.isProcessingPayment.set(true);

      this.paymentService.initializeQrPayment(orderId).subscribe({
        next: (response) => {
          window.location.href = response.bankUrl;
        },
        error: (err) => {
          console.error('Error initializing QR payment:', err);
          this.isProcessingPayment.set(false);
          this.selectedMethod.set(null);
        },
      });
    } else if (method.toLowerCase() === 'card') {
      this.isProcessingPayment.set(true);

      this.paymentService.initializeCardPayment(orderId).subscribe({
        next: (response) => {
          window.location.href = response.bankUrl;
        },
        error: (err) => {
          console.error('Error initializing Card payment:', err);
          this.isProcessingPayment.set(false);
          this.selectedMethod.set(null);
        },
      });
    }
  }
}
