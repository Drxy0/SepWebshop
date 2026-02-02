import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Constants } from '../../constants/constants';
import { IRegisterRequest, ILoginRequest, ILoginResponse } from '../../models/interfaces/user';

@Injectable({
  providedIn: 'root',
})
export class User {
  // Signal za praćenje stanja osvežavanja
  public isRefreshing = signal<boolean>(false);

  // Subject ostaje jer nam treba RxJS "buffer" za zahteve koji čekaju
  public refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(private http: HttpClient) {}

  registerUser(obj: IRegisterRequest): Observable<string> {
    return this.http.post(environment.api_url + Constants.API_METHOD.REGISTER_USER, obj, {
      responseType: 'text',
    });
  }

  loginUser(obj: ILoginRequest): Observable<ILoginResponse> {
    return this.http.post<ILoginResponse>(
      environment.api_url + Constants.API_METHOD.LOGIN_USER,
      obj,
    );
  }

  refreshToken(refreshToken: string): Observable<ILoginResponse> {
    return this.http.post<ILoginResponse>(
      environment.api_url + Constants.API_METHOD.REFRESH_TOKEN,
      { refreshToken },
    );
  }
}
