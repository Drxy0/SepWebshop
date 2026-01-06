import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Constants } from '../../constants/constants';
import { ILoginRequest, ILoginResponse } from '../../models/interfaces/user';

@Injectable({
  providedIn: 'root',
})
export class User {
  // Signal za praćenje stanja osvežavanja
  public isRefreshing = signal<boolean>(false);

  // Subject ostaje jer nam treba RxJS "buffer" za zahteve koji čekaju
  public refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(private http: HttpClient) {}

  loginUser(obj: ILoginRequest): Observable<ILoginResponse> {
    return this.http.post<ILoginResponse>(
      environment.data_service_api_url + Constants.API_METHOD.LOGIN_USER,
      obj
    );
  }
}
