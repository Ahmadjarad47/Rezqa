import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

// Create a service to manage the refresh token state
const refreshTokenState = {
  isRefreshing: false,
  refreshTokenSubject: new BehaviorSubject<any>(null)
};

// Helper function to handle refresh token errors
const handleRefreshTokenError = (router: Router) => {
  refreshTokenState.refreshTokenSubject.next(null);
  router.navigate(['/identity/login'], {
    queryParams: { returnUrl: router.url },
  });
};

// Helper function to refresh the token
const refreshToken = (): Observable<any> => {
  return new Observable((observer) => {
    const xhr = new XMLHttpRequest();
    xhr.open('POST', `${environment.apiUrl}Auth/refresh-token`, true);
    xhr.withCredentials = true;

    xhr.onload = () => {
      if (xhr.status === 200) {
        observer.next(xhr.response);
        observer.complete();
      } else {
        observer.error(xhr.response);
      }
    };

    xhr.onerror = () => {
      observer.error('Network error occurred');
    };

    xhr.send();
  });
};

// Helper function to handle 401 errors
const handle401Error = (request: HttpRequest<any>, next: HttpHandlerFn, router: Router): Observable<any> => {
  if (!refreshTokenState.isRefreshing) {
    refreshTokenState.isRefreshing = true;
    refreshTokenState.refreshTokenSubject.next(null);

    return refreshToken().pipe(
      switchMap(() => {
        refreshTokenState.isRefreshing = false;
        refreshTokenState.refreshTokenSubject.next(true);
        return next(request);
      }),
      catchError((error) => {
        refreshTokenState.isRefreshing = false;
        handleRefreshTokenError(router);
        return throwError(() => error);
      })
    );
  }

  return refreshTokenState.refreshTokenSubject.pipe(
    filter((result) => result !== null),
    take(1),
    switchMap(() => next(request))
  );
};

export const authInterceptor: HttpInterceptorFn = (
  request: HttpRequest<any>,
  next: HttpHandlerFn
) => {
  const router = inject(Router);

  // Add withCredentials to all requests
  request = request.clone({
    withCredentials: true,
  });

  // Skip token refresh for auth-related endpoints
  if (
    request.url.toLowerCase().includes('/api/auth/refresh-token') ||
    request.url.toLowerCase().includes('/api/auth/login') ||
    request.url.toLowerCase().includes('/api/auth/is-auth')
  ) {
    return next(request);
  }

  return next(request).pipe(
    tap((event) => {
      if (event instanceof HttpResponse) {
        if (event.url?.toLowerCase().includes('/api/auth/refresh-token')) {
          refreshTokenState.refreshTokenSubject.next(true);
        }
      }
    }),
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        if (error.url?.toLowerCase().includes('/api/auth/refresh-token')) {
          handleRefreshTokenError(router);
          return throwError(() => error);
        }
        return handle401Error(request, next, router);
      }
      return throwError(() => error);
    })
  );
};
