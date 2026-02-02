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
  formValues = signal({ cardHolder: '', cardNumber: '', expiry: '', cvv: '' });

  loading = computed(() => this.loadingSignal());
  payment = computed(() => this.paymentSignal());
  error = computed(() => this.errorSignal());

  private paymentRequestId!: string;

  form: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      cardNumber: [''],
      expiry: [''],
      cvv: [''],
      cardHolder: ['']
    });
  }

  ngOnInit(): void {
    this.paymentRequestId =
      this.route.snapshot.paramMap.get('paymentRequestId')!;

    // Subscribe to form value changes
    this.form.valueChanges.subscribe(values => {
      this.formValues.set({
        cardHolder: values.cardHolder || '',
        cardNumber: values.cardNumber || '',
        expiry: values.expiry || '',
        cvv: values.cvv || ''
      });
    });

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
    detectCardBrand(this.formValues().cardNumber)
  );

  isFormValid = computed(() => {
    const v = this.formValues();

    const holderValid = v.cardHolder.trim().length > 2;
    const cardNumberValid = isValidLuhn(v.cardNumber);
    const brandValid = this.cardBrand() !== 'UNKNOWN';
    const expiryValid = isExpiryValid(v.expiry);
    const cvvValid = /^\d{3}$/.test(v.cvv);

    console.log('Form validation:', {
      holderValid,
      cardNumberValid,
      brandValid,
      expiryValid,
      cvvValid,
      cardHolder: v.cardHolder,
      cardNumber: v.cardNumber,
      brand: this.cardBrand(),
      expiry: v.expiry,
      cvv: v.cvv
    });

    return (
      holderValid &&
      cardNumberValid &&
      brandValid &&
      expiryValid &&
      cvvValid
    );
  });

  submitDisabled = computed(() =>
    !this.isFormValid() || this.submitting()
  );

  handleSubmit() {
    if (this.submitDisabled()) return;

    this.hasAttempted.set(true);
    this.submitting.set(true);
    this.errorSignal.set(null);

    const raw = this.form.getRawValue();

    // Parse expiry from "MM/YY" format
    const [month, year] = raw.expiry.split('/').map(Number);

    const paymentData: PayByCardRequest = {
      cardNumber: raw.cardNumber,
      cardHolderName: raw.cardHolder,
      expiryMonth: month,
      expiryYear: year,
      cardHolder: raw.cardHolder,
      cvv: raw.cvv
    };

    this.paymentService.submitPayment(this.paymentRequestId, paymentData)
      .subscribe({
        next: (redirectUrl: string) => {
          window.location.href = redirectUrl;
        },
        error: () => {
          this.errorSignal.set('Payment failed. Please try again.');
          this.submitting.set(false);
          this.hasAttempted.set(false); // Allow retry
        }
      });
  }

}