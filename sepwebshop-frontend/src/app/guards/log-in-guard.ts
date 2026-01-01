import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Constants } from '../constants/constants';

export const logInGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN);
  if (token != null) {
    router.navigate(['/index']);
    return false;
  } else {
    return true;
  }
};
