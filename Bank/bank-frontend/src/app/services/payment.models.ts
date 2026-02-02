// Card Payment Models
export interface CardPaymentRequestDto {
  paymentRequestId: string;
  amount: number;
  currency: string;
  status: PaymentRequestStatus;
  expiresAt: string;
}

export interface PayByCardRequest {
  cardNumber: string;
  cardHolderName: string;
  expiryMonth: number;
  expiryYear: number;
  cardHolder: string;
  cvv: string;
}

// QR Payment Models
export interface QrPaymentResponseDto {
  paymentRequestId: string;
  qrCodeBase64: string | null;
  status: PaymentRequestStatus;
  stan: string;
  expiresAt: string;
  amount?: number;
  currency?: string;
}

export interface QrPaymentStatusDto {
  paymentRequestId: string;
  status: PaymentRequestStatus;
  amount: number;
  currency: string;
  expiresAt: string;
  transactionId?: string;
  completedAt?: string;
}

// Enums
export type PaymentRequestStatus = 'Pending' | 'Success' | 'Failed' | 'Expired';

// For backend processing (simulation)
export interface ProcessQrPaymentRequest {
  customerAccountNumber?: string;
}
