import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  FormControl
} from '@angular/forms';
import { CardBrand, CardPaymentRequestDto } from './card-payment.models';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-card-payment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './card-payment.html',
  styleUrl: './card-payment.scss'
})
export class CardPayment implements OnInit {

  paymentSignal = signal<CardPaymentRequestDto | null>(null);
  loadingSignal = signal(true);
  errorSignal = signal<string | null>(null);
  submitting = signal(false);
  hasAttempted = signal(false);

  // Add computed properties for template access
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
    private http: HttpClient,
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

    this.http
      .get<CardPaymentRequestDto>(
        `https://localhost:7278/api/payments/${this.paymentRequestId}`
      )
      .subscribe({
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

  // --------------------
  // Computed
  // --------------------

  brand = computed<CardBrand>(() =>
    this.detectCardBrand(this.form.controls.cardNumber.value)
  );

  isFormValid = computed(() => {
    const v = this.form.getRawValue();

    return (
      v.cardHolder.trim().length > 2 &&
      this.isValidLuhn(v.cardNumber) &&
      this.brand() !== 'UNKNOWN' &&
      this.isExpiryValid(v.expiry) &&
      /^\d{3}$/.test(v.cvv)
    );
  });

  submitDisabled = computed(() =>
    !this.isFormValid() || this.submitting() || this.hasAttempted()
  );

  // --------------------
  // Submit
  // --------------------

  handleSubmit() {
    if (this.submitDisabled()) return;

    this.hasAttempted.set(true);
    this.submitting.set(true);
    this.errorSignal.set(null);

    this.http
      .post<string>(
        `https://localhost:7278/api/payments/${this.paymentRequestId}/pay`,
        this.form.getRawValue()
      )
      .subscribe({
        next: redirectUrl => {
          window.location.href = redirectUrl;
        },
        error: () => {
          this.errorSignal.set('Payment failed. Please try again.');
          this.submitting.set(false);
        }
      });
  }

  // --------------------
  // Helpers
  // --------------------

  detectCardBrand(cardNumber: string): CardBrand {
    const digits = cardNumber.replace(/\s+/g, '');
    if (/^4\d*/.test(digits)) return 'VISA';
    if (/^(5[1-5]|2[2-7])/.test(digits)) return 'MASTERCARD';
    return 'UNKNOWN';
  }

  isExpiryValid(expiry: string): boolean {
    if (!/^\d{2}\/\d{2}$/.test(expiry)) return false;
    const [month, year] = expiry.split('/').map(Number);
    if (month < 1 || month > 12) return false;
    const now = new Date();
    const expiryDate = new Date(2000 + year, month);
    return expiryDate > now;
  }

  isValidLuhn(cardNumber: string): boolean {
    const digits = cardNumber.replace(/\s+/g, '');
    let sum = 0;
    let alternate = false;
    for (let i = digits.length - 1; i >= 0; i--) {
      let n = Number(digits[i]);
      if (alternate) {
        n *= 2;
        if (n > 9) n -= 9;
      }
      sum += n;
      alternate = !alternate;
    }
    return sum % 10 === 0;
  }
}