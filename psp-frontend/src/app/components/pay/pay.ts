import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { User } from '../../services/user/user';
import { CommonModule } from '@angular/common';
import { PaymentService } from '../../services/payment/payment-service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-pay',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pay.html',
})
export class Pay implements OnInit {
  private route = inject(ActivatedRoute);
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
    const oId = this.orderId();
    if (this.isProcessingPayment()) return;

    this.selectedMethod.set(method);

    if (method === 'QR' && oId) {
      this.isProcessingPayment.set(true);
      const cleanOrderId = oId.replace(/-/g, '');

      this.paymentService.initializeQrPayment(cleanOrderId).subscribe({
        next: (response) => {
          window.location.href = response.bankUrl;
        },
        error: (err) => {
          console.error('Greška pri inicijalizaciji QR plaćanja:', err);
          this.isProcessingPayment.set(false);
          this.selectedMethod.set(null);
        },
      });
    } else {
      console.log(`Starting payment for ${this.orderId()} using ${method}`);
    }
  }
}
