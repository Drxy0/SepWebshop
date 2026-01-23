export interface CardPaymentRequestDto {
  amount: number;
  currency: string;
}

export interface QrPaymentResponseDto {
  paymentRequestId: string;
  qrCodeBase64: string;
}

export interface PayByCardRequest {
  cardNumber: string;
  cardHolderName: string;
  expiryMonth: number;
  expiryYear: number;
  cardHolder: string;
  cvv: string;
}
