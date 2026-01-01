import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Constants } from '../../constants/constants';
import { IRegisterRequest, ILoginRequest, ILoginResponse } from '../../models/interfaces/user';
@Injectable({
  providedIn: 'root',
})
export class User {
  constructor(private http: HttpClient) {}

  registerUser(obj: IRegisterRequest): Observable<void> {
    return this.http.post<void>(environment.api_url + Constants.API_METHOD.REGISTER_USER, obj);
  }
  loginUser(obj: ILoginRequest): Observable<ILoginResponse> {
    return this.http.post<ILoginResponse>(
      environment.api_url + Constants.API_METHOD.LOGIN_USER,
      obj
    );
  }
}
