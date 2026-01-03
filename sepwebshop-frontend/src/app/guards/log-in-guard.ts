import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Constants } from '../constants/constants';

function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return Date.now() > payload.exp * 1000;
  } catch {
    return true;
  }
}

export const logInGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN);
  if (token && !isTokenExpired(token)) {
    router.navigate(['/index']);
    return false;
  }

  localStorage.removeItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN);
  localStorage.removeItem(Constants.LOCAL_STORAGE_REFRESH_TOKEN);

  return true;
};
