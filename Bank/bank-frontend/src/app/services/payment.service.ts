import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CardPaymentRequestDto {
  amount: number;
  currency: string;
}

export interface QrPaymentResponseDto {
  paymentRequestId: string;
  qrCodeBase64: string;
}

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  private baseUrl = 'https://localhost:7278/api/payments';

  constructor(private http: HttpClient) {}

  getPaymentRequest(paymentRequestId: string): Observable<CardPaymentRequestDto> {
    return this.http.get<CardPaymentRequestDto>(
      `${this.baseUrl}/${paymentRequestId}`
    );
  }

  submitPayment(paymentRequestId: string, paymentData: any): Observable<string> {
    return this.http.post<string>(
      `${this.baseUrl}/${paymentRequestId}/pay`,
      paymentData,
      { responseType: 'text' as 'json' }
    );
  }

  getQrCode(paymentRequestId: string): Observable<QrPaymentResponseDto> {
  return this.http.post<QrPaymentResponseDto>(
    `${this.baseUrl}/${paymentRequestId}/qr`,
    {}
  );
}
}