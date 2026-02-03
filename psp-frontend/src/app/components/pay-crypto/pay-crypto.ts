import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CheckPaymentStatusResponse } from '../../models/interfaces/payment';
import { PaymentService } from '../../services/payment/payment-service';

@Component({
  selector: 'app-pay-crypto',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pay-crypto.html',
  styleUrls: ['./pay-crypto.css'],
})
export class PayCrypto {
  private paymentService = inject(PaymentService);

  orderId = signal<string | null>(null);
  qrImageUrl = signal<string | null>(null);
  isLoading = signal(true);
  isProccessing = signal(false);

  constructor() {
    const state = history.state as { qrUrl?: string; orderId?: string };

    if (state?.orderId) {
      this.orderId.set(state.orderId);
    }

    if (state?.qrUrl) {
      const currentUrl = this.qrImageUrl();
      if (currentUrl) {
        URL.revokeObjectURL(currentUrl);
      }
      this.qrImageUrl.set(state.qrUrl);
      this.isLoading.set(false);
    }
  }

  simulatePayment(): void {
    const orderId = this.orderId();
    if (!orderId) return;

    this.isProccessing.set(true);

    this.paymentService.simulateCryptoPayment(orderId).subscribe({
      next: (res: CheckPaymentStatusResponse) => {
        if (res.redirectUrl) {
          window.location.href = res.redirectUrl;
        }
      },
      error: (err) => {
        console.error('Simulation failed:', err);
      },
      complete: () => {
        this.isProccessing.set(false);
      },
    });
  }

  ngOnDestroy() {
    const url = this.qrImageUrl();
    if (url) {
      URL.revokeObjectURL(url);
    }
  }

  simulateTransaction() {
    const id = this.orderId();
    if (!id) return;

    this.isSimulating.set(true);

    this.paymentService.simulateCryptoPaymentComplete(id).subscribe({
      next: (response) => {
        this.isSimulating.set(false);

        if (response.redirectUrl) {
          window.location.href = response.redirectUrl;
        } else {
          alert('Simulation completed, but no redirect URL provided.');
        }
      },
      error: (err) => {
        console.error('Simulation failed', err);
        alert('Simulation failed. Check console.');
        this.isSimulating.set(false);
      },
    });
  }
}
