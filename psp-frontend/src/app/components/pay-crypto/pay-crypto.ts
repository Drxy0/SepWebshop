import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CheckPaymentStatusResponse } from '../../models/interfaces/payment';
import { PaymentService } from '../../services/payment/payment-service';
import { Subscription, timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-pay-crypto',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pay-crypto.html',
  styleUrls: ['./pay-crypto.css'],
})
export class PayCrypto {
  private paymentService = inject(PaymentService);
  private statusPollingSub?: Subscription;

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

    if (state?.orderId) {
      this.orderId.set(state.orderId);
      this.startPolling();
    }
  }

  private startPolling(): void {
    const orderId = this.orderId();
    if (!orderId) return;

    this.statusPollingSub = timer(0, 60_000)
      .pipe(switchMap(() => this.paymentService.checkCryptoPaymentStatus(orderId)))
      .subscribe({
        next: (res: CheckPaymentStatusResponse) => {
          if (res.redirectUrl) {
            this.stopPolling();
            window.location.href = res.redirectUrl;
          }
        },
        error: (err) => {
          console.error('Status check failed:', err);
        },
      });
  }

  private stopPolling(): void {
    this.statusPollingSub?.unsubscribe();
    this.statusPollingSub = undefined;
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
    this.stopPolling();

    const url = this.qrImageUrl();
    if (url) {
      URL.revokeObjectURL(url);
    }
  }
}
