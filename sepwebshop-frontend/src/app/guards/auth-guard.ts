import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Constants } from '../constants/constants';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem(Constants.LOCAL_STORAGE_TOKEN);
  if (token != null) {
    return true;
  } else {
    router.navigate(['/login']);
    return false;
  }
};
