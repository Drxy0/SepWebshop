import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Constants } from '../../constants/constants';
import { IAddCarRequest, ICarResponse } from '../../models/interfaces/car';

@Injectable({
  providedIn: 'root',
})
export class CarsService {
  constructor(private http: HttpClient) {}

  addCar(obj: IAddCarRequest): Observable<void> {
    return this.http.post<void>(environment.api_url + Constants.API_METHOD.CARS, obj);
  }

  getAllCars(): Observable<ICarResponse[]> {
    return this.http.get<ICarResponse[]>(environment.api_url + Constants.API_METHOD.CARS);
  }

  getCar(id: string): Observable<ICarResponse> {
    return this.http.get<ICarResponse>(`${environment.api_url}${Constants.API_METHOD.CARS}/${id}`);
  }

  updateCar(id: string, obj: IAddCarRequest): Observable<void> {
    return this.http.put<void>(`${environment.api_url}${Constants.API_METHOD.CARS}/${id}`, obj);
  }

  deleteCar(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.api_url}${Constants.API_METHOD.CARS}/${id}`);
  }
}
