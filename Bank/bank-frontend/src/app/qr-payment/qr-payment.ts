import { Component, signal, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { catchError, of, finalize, interval, Subscription } from 'rxjs';
import { PaymentService } from '../services/payment.service';
import { QrPaymentResponseDto, QrPaymentStatusDto } from '../services/payment.models';

@Component({
  selector: 'app-qr-payment',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './qr-payment.html',
  styleUrl: './qr-payment.scss',
})
export class QrPayment implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private paymentService = inject(PaymentService);
  private pollingSubscription?: Subscription;

  qr = signal<QrPaymentResponseDto | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  paymentStatus = signal<QrPaymentStatusDto | null>(null);
  isPolling = signal(false);
  simulatingPayment = signal(false);

  simulateMobilePayment(): void {
    const paymentRequestId = this.qr()?.paymentRequestId;
    if (!paymentRequestId) return;

    this.simulatingPayment.set(true);
    const testAccount = '123-456789-78';

    this.paymentService
      .processQrPayment(paymentRequestId, testAccount)
      .pipe(
        catchError((err) => {
          console.error('Payment processing error:', err);
          this.error.set('Payment simulation failed');
          return of(null);
        }),
        finalize(() => {
          this.simulatingPayment.set(false);
        }),
      )
      .subscribe((result) => {
        if (result) {
          console.log('Payment processed:', result);
          // Force immediate status check
          this.checkPaymentStatus(paymentRequestId);
        }
      });
  }

  ngOnInit(): void {
    const paymentRequestId = this.route.snapshot.paramMap.get('paymentRequestId');

    if (!paymentRequestId) {
      this.error.set('Payment request ID not provided');
      this.loading.set(false);
      return;
    }

    this.loadQrCode(paymentRequestId);
  }

  ngOnDestroy(): void {
    this.cleanup();
  }

  private loadQrCode(paymentRequestId: string): void {
    this.paymentService
      .getQrCode(paymentRequestId)
      .pipe(
        catchError((err) => {
          console.error('QR code generation error:', err);
          this.error.set('Unable to generate QR code. Please try again.');
          return of(null);
        }),
        finalize(() => {
          this.loading.set(false);
        }),
      )
      .subscribe((res) => {
        if (res) {
          this.qr.set(res);
          // Immediately check status to get amount/currency
          this.checkPaymentStatus(paymentRequestId);
          this.startStatusPolling();
        }
      });
  }

  private startStatusPolling(): void {
    this.stopStatusPolling();
    this.isPolling.set(true);

    const paymentRequestId = this.qr()?.paymentRequestId;
    if (!paymentRequestId) return;

    // Poll every 60 seconds
    this.pollingSubscription = interval(60_000).subscribe(() => {
      this.checkPaymentStatus(paymentRequestId);
    });
  }

  private stopStatusPolling(): void {
    if (this.pollingSubscription) {
      this.pollingSubscription.unsubscribe();
      this.pollingSubscription = undefined;
    }
    this.isPolling.set(false);
  }

  private checkPaymentStatus(paymentRequestId: string): void {
    this.paymentService
      .getQrPaymentStatus(paymentRequestId)
      .pipe(
        catchError((err) => {
          console.error('Error checking payment status:', err);
          return of(null);
        }),
      )
      .subscribe((status) => {
        if (status) {
          this.paymentStatus.set(status);

          // Stop polling if payment is completed, failed, or expired
          if (status.status !== 'Pending') {
            this.stopStatusPolling();
          }
        }
      });
  }

  private cleanup(): void {
    this.stopStatusPolling();
  }
}
