import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CardPaymentRequestDto,
  PayByCardRequest,
  QrPaymentResponseDto,
  QrPaymentStatusDto,
} from './payment.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  private baseUrl = environment.bank_api_url + 'api/bank/payments';

  constructor(private http: HttpClient) {}

  getPaymentRequest(paymentRequestId: string): Observable<CardPaymentRequestDto> {
    return this.http.get<CardPaymentRequestDto>(`${this.baseUrl}/${paymentRequestId}`);
  }

  submitPayment(paymentRequestId: string, paymentData: PayByCardRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/card/${paymentRequestId}`, paymentData, {
      responseType: 'text' as 'json',
    });
  }

  getQrCode(paymentRequestId: string): Observable<QrPaymentResponseDto> {
    return this.http.post<QrPaymentResponseDto>(`${this.baseUrl}/qr/${paymentRequestId}`, {});
  }

  getQrPaymentStatus(paymentRequestId: string): Observable<QrPaymentStatusDto> {
    return this.http.get<QrPaymentStatusDto>(`${this.baseUrl}/qr/${paymentRequestId}/status`);
  }

  // For simulation only
  processQrPayment(
    paymentRequestId: string,
    customerAccountNumber?: string,
  ): Observable<QrPaymentResponseDto> {
    return this.http.post<QrPaymentResponseDto>(`${this.baseUrl}/qr/${paymentRequestId}/process`, {
      customerAccountNumber,
    });
  }
}
