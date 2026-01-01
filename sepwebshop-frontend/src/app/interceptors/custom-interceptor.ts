import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, switchMap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { User } from '../services/user/user';
import { Constants } from '../constants/constants';

export const customInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const userService = inject(User);

  const accessToken = localStorage.getItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN);

  let authReq = req;
  if (accessToken) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`,
      },
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401) {
        return throwError(() => error);
      }

      const refreshToken = localStorage.getItem(Constants.LOCAL_STORAGE_REFRESH_TOKEN);

      if (!refreshToken) {
        logout(router);
        return throwError(() => error);
      }

      return userService.refreshToken(refreshToken).pipe(
        switchMap((res) => {
          localStorage.setItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN, res.accessToken);
          localStorage.setItem(Constants.LOCAL_STORAGE_REFRESH_TOKEN, res.refreshToken);

          const retryReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${res.accessToken}`,
            },
          });

          return next(retryReq);
        }),
        catchError((refreshError) => {
          logout(router);
          return throwError(() => refreshError);
        })
      );
    })
  );
};

function logout(router: Router) {
  localStorage.removeItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN);
  localStorage.removeItem(Constants.LOCAL_STORAGE_REFRESH_TOKEN);
  router.navigate(['/login']);
}
