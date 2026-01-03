import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, switchMap, throwError, filter, take } from 'rxjs';
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

      // Provera Signala pomoću ()
      if (!userService.isRefreshing()) {
        // Postavljanje Signala pomoću .set()
        userService.isRefreshing.set(true);
        userService.refreshTokenSubject.next(null);

        return userService.refreshToken(refreshToken).pipe(
          switchMap((res: any) => {
            userService.isRefreshing.set(false);

            localStorage.setItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN, res.accessToken);
            localStorage.setItem(Constants.LOCAL_STORAGE_REFRESH_TOKEN, res.refreshToken);

            userService.refreshTokenSubject.next(res.accessToken);

            return next(
              req.clone({
                setHeaders: { Authorization: `Bearer ${res.accessToken}` },
              })
            );
          }),
          catchError((refreshError) => {
            userService.isRefreshing.set(false);
            logout(router);
            return throwError(() => refreshError);
          })
        );
      } else {
        // Ako je refresh u toku, čekamo RxJS subject
        return userService.refreshTokenSubject.pipe(
          filter((token) => token !== null),
          take(1),
          switchMap((token) => {
            return next(
              req.clone({
                setHeaders: { Authorization: `Bearer ${token}` },
              })
            );
          })
        );
      }
    })
  );
};

function logout(router: Router) {
  localStorage.removeItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN);
  localStorage.removeItem(Constants.LOCAL_STORAGE_REFRESH_TOKEN);
  router.navigate(['/login']);
}
