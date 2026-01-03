import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Constants } from '../../constants/constants';
import { IAddInsuranceRequest, IInsuranceResponse } from '../../models/interfaces/insurance';

@Injectable({
  providedIn: 'root',
})
export class InsurancesService {
  private http = inject(HttpClient);
  private apiUrl = environment.api_url + Constants.API_METHOD.INSURANCES;

  addInsurance(obj: IAddInsuranceRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl, obj);
  }

  getAllInsurances(): Observable<IInsuranceResponse[]> {
    return this.http.get<IInsuranceResponse[]>(this.apiUrl);
  }

  getInsurance(id: string): Observable<IInsuranceResponse> {
    return this.http.get<IInsuranceResponse>(`${this.apiUrl}/${id}`);
  }

  updateInsurance(id: string, obj: IAddInsuranceRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, obj);
  }

  deleteInsurance(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
