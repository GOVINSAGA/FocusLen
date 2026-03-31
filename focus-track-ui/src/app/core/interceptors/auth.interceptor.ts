import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth';

/**
 * JWT Auth Interceptor
 * - Attaches Bearer token to all outgoing requests
 * - Catches 4xx/5xx responses and surfaces a user-friendly error
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  const authedReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      let userMessage = 'An unexpected error occurred. Please try again.';

      if (error.status === 401) {
        userMessage = 'Your session has expired. Please log in again.';
        authService.logout();
      } else if (error.status === 403) {
        userMessage = 'You do not have permission to perform this action.';
      } else if (error.status === 409) {
        userMessage = error.error?.error ?? 'A conflict occurred.';
      } else if (error.status >= 400 && error.status < 500) {
        userMessage = error.error?.error ?? 'Invalid request. Please check your input.';
      } else if (error.status >= 500) {
        userMessage = 'Server error. Please try again later.';
      }

      // Attach friendly message so components can use it directly
      const enrichedError = { ...error, friendlyMessage: userMessage };
      return throwError(() => enrichedError);
    })
  );
};
