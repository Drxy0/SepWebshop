import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  importProvidersFrom,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideRouter, RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { customInterceptor } from './interceptors/custom-interceptor';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    importProvidersFrom(HttpClientModule, BrowserAnimationsModule, RouterModule),
    provideHttpClient(withInterceptors([customInterceptor])),
  ],
};
