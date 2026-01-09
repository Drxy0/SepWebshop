import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Constants } from '../../constants/constants';
import { ILoginRequest, ILoginResponse } from '../../models/interfaces/user';
import { IPaymentMethod } from '../../models/interfaces/payment';

@Injectable({
  providedIn: 'root',
})
export class User {
  constructor(private http: HttpClient) {}

  loginUser(obj: ILoginRequest): Observable<ILoginResponse> {
    return this.http.post<ILoginResponse>(
      environment.data_service_api_url + Constants.API_METHOD.LOGIN_USER,
      obj
    );
  }

  getPaymentMethods(): Observable<IPaymentMethod[]> {
    return this.http.get<IPaymentMethod[]>(
      environment.data_service_api_url + Constants.API_METHOD.PAYMENT_METHODS
    );
  }

  updatePaymentMethods(methodIds: number[]): Observable<any> {
    return this.http.post(environment.data_service_api_url + Constants.API_METHOD.PAYMENT_METHODS, {
      paymentMethodIds: methodIds,
    });
  }

  getActiveMethods(merchantId: string): Observable<string[]> {
    return this.http.get<string[]>(
      `${environment.data_service_api_url}Payment/methods/${merchantId}`
    );
  }
}
