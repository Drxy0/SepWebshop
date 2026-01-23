import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { catchError, of, finalize } from 'rxjs';
import { PaymentService } from '../services/payment.service';
import { QrPaymentResponseDto } from '../services/payment.models';

@Component({
  selector: 'app-qr-payment',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './qr-payment.html',
  styleUrl: './qr-payment.scss'
})
export class QrPayment implements OnInit {
  private route = inject(ActivatedRoute);
  private paymentService = inject(PaymentService);

  qr = signal<QrPaymentResponseDto | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    const paymentRequestId = this.route.snapshot.paramMap.get('paymentRequestId');

    if (!paymentRequestId) {
      this.error.set('Payment request ID not provided');
      this.loading.set(false);
      return;
    }

    this.paymentService.getQrCode(paymentRequestId)
      .pipe(
        catchError((err) => {
          console.error('QR code generation error:', err);
          this.error.set('Unable to generate QR code. Please try again.');
          return of(null);
        }),
        finalize(() => {
          this.loading.set(false);
        })
      )
      .subscribe((res) => {
        if (res) {
          this.qr.set(res);
        }
      });
  }
}