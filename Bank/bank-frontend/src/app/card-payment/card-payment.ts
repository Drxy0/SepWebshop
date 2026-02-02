import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  FormControl
} from '@angular/forms';
import { CardBrand } from './card-payment.models';
import { detectCardBrand, isExpiryValid, isValidLuhn } from './card-payment.util';
import { PaymentService } from '../services/payment.service';
import { CardPaymentRequestDto, PayByCardRequest } from '../services/payment.models';

@Component({
  selector: 'app-card-payment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './card-payment.html',
  styleUrl: './card-payment.scss'
})
export class CardPayment implements OnInit {
  private paymentService = inject(PaymentService);

  paymentSignal = signal<CardPaymentRequestDto | null>(null);
  loadingSignal = signal(true);
  errorSignal = signal<string | null>(null);
  submitting = signal(false);
  hasAttempted = signal(false);

  loading = computed(() => this.loadingSignal());
  payment = computed(() => this.paymentSignal());
  error = computed(() => this.errorSignal());

  private paymentRequestId!: string;

  form: FormGroup<{
    cardNumber: FormControl<string>;
    expiry: FormControl<string>;
    cvv: FormControl<string>;
    cardHolder: FormControl<string>;
  }>;

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder
  ) {
    this.form = this.fb.nonNullable.group({
      cardNumber: [''],
      expiry: [''],
      cvv: [''],
      cardHolder: ['']
    });
  }

  ngOnInit(): void {
    this.paymentRequestId =
      this.route.snapshot.paramMap.get('paymentRequestId')!;

    this.paymentService.getPaymentRequest(this.paymentRequestId).subscribe({
      next: res => {
        this.paymentSignal.set(res);
        this.loadingSignal.set(false);
      },
      error: () => {
        this.errorSignal.set('Payment request not found');
        this.loadingSignal.set(false);
      }
    });
  }

  cardBrand = computed<CardBrand>(() =>
    detectCardBrand(this.form.controls.cardNumber.value)
  );

  isFormValid = computed(() => {
    const v = this.form.getRawValue();

    return (
      v.cardHolder.trim().length > 2 &&
      isValidLuhn(v.cardNumber) &&
      this.cardBrand() !== 'UNKNOWN' &&
      isExpiryValid(v.expiry) &&
      /^\d{3}$/.test(v.cvv)
    );
  });

  submitDisabled = computed(() =>
    !this.isFormValid() || this.submitting() || this.hasAttempted()
  );

  handleSubmit() {
    if (this.submitDisabled()) return;

    this.hasAttempted.set(true);
    this.submitting.set(true);
    this.errorSignal.set(null);

    const raw = this.form.getRawValue();

    // Convert expiry from "MM/YY" format to match backend expectations
    const paymentData: PayByCardRequest = {
      cardNumber: raw.cardNumber,
      expirationDate: raw.expiry, // Keep as "MM/YY" format
      cvv: raw.cvv,
      cardHolderName: raw.cardHolder
    };

    this.paymentService.submitPayment(this.paymentRequestId, paymentData)
      .subscribe({
        next: (redirectUrl: string) => {
          window.location.href = redirectUrl;
        },
        error: () => {
          this.errorSignal.set('Payment failed. Please try again.');
          this.submitting.set(false);
        }
      });
  }

}