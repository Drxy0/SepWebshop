import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { IInitPaymentResponse } from '../../models/interfaces/payment';

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  constructor(private http: HttpClient) {}

  initializeQrPayment(merchantOrderId: string): Observable<IInitPaymentResponse> {
    const url = `${environment.qr_service_api_url}Payment/init`;

    const body = {
      merachanOrderId: merchantOrderId,
    };

    return this.http.post<IInitPaymentResponse>(url, body);
  }
}
