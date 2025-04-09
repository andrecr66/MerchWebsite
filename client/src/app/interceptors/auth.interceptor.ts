import { HttpInterceptorFn } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core'; // Import PLATFORM_ID
import { isPlatformBrowser } from '@angular/common'; // Import isPlatformBrowser
import { AuthService } from '../services/auth.service'; // Import AuthService

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Inject AuthService and PLATFORM_ID
  const authService = inject(AuthService);
  const platformId = inject(PLATFORM_ID);

  // Only attempt to get token and modify request if in browser
  if (isPlatformBrowser(platformId)) {
    const authToken = authService.getToken();

    // Clone the request and add the authorization header if token exists
    if (authToken) {
      const authReq = req.clone({
        headers: req.headers.set('Authorization', `Bearer ${authToken}`)
      });
      // Pass cloned request with header to the next handler.
      return next(authReq);
    }
  }

  // If not in browser or no token, pass the original request without modification
  return next(req);
};
