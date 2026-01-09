import { HttpInterceptorFn } from '@angular/common/http';
import { Constants } from '../constants/constants';

export const customInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN);

  if (token) {
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
    return next(cloned);
  }

  return next(req);
};
