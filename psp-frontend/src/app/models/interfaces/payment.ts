export interface IPaymentMethod {
  id: number;
  name: string;
  selected: boolean;
}

export interface IUpdatePaymentRequest {
  paymentMethodIds: number[];
}

export interface IInitPaymentResponse {
  bankUrl: string;
}

export interface CryptoPaymentResponse {
  qrCodeBase64: string;
  merchantOrderId: string;
}

export interface CheckPaymentStatusResponse {
  status: string;
  bitcoinAmount: number;
  transactionId: string | null;
  webshopNotified: boolean;
  redirectUrl: string | null;
}
