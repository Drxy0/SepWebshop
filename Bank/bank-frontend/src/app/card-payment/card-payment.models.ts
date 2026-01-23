export type CardBrand = 'VISA' | 'MASTERCARD' | 'UNKNOWN';

export interface CardFormData {
  cardNumber: string;
  expiry: string;
  cvv: string;
  cardHolder: string;
}

export interface CardPaymentRequestDto {
  amount: number;
  currency: string;
}