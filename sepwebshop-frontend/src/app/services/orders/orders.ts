import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Constants } from '../../constants/constants';
import { IOrderCreateResponse, IOrderResponse } from '../../models/interfaces/order';

@Injectable({
  providedIn: 'root',
})
export class OrdersService {
  private http = inject(HttpClient);

  getOrdersByCarId(carId: string): Observable<IOrderResponse[]> {
    return this.http.get<IOrderResponse[]>(
      `${environment.api_url}${Constants.API_METHOD.ORDERS}/car/${carId}`
    );
  }

  createOrder(payload: any): Observable<IOrderCreateResponse> {
    return this.http.post<IOrderCreateResponse>(
      `${environment.api_url}${Constants.API_METHOD.ORDERS}`,
      payload
    );
  }
}
