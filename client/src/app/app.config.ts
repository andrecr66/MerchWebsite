import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http'; // Import withInterceptors
import { authInterceptor } from './interceptors/auth.interceptor'; // Import the interceptor

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideClientHydration(withEventReplay()),
    // Provide HttpClient with fetch and the auth interceptor
    provideHttpClient(withFetch(), withInterceptors([authInterceptor]))
  ]
};
