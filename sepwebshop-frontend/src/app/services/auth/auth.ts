import { Injectable } from '@angular/core';
import { Constants } from '../../constants/constants';

@Injectable({
  providedIn: 'root',
})
export class Auth {
  getAccessToken(): string | null {
    return localStorage.getItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN);
  }

  logout() {
    localStorage.removeItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN);
    localStorage.removeItem(Constants.LOCAL_STORAGE_REFRESH_TOKEN);
  }

  private decodeToken(): any | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch {
      return null;
    }
  }

  getRole(): string | null {
    const decoded = this.decodeToken();
    if (!decoded) return null;

    return decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? null;
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }

  isUser(): boolean {
    return this.getRole() === 'User';
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  getUserId(): string | null {
    const decoded = this.decodeToken();
    return decoded ? decoded.sub : null;
  }
}
