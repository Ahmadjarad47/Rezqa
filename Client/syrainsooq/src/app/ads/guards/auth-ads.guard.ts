import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ActivatedRouteSnapshot, CanActivate, CanActivateFn, Router, UrlTree } from '@angular/router';
import { environment } from '@environments/environment.development';
import { BehaviorSubject, catchError, filter, map, Observable, of, switchMap, take } from 'rxjs';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class authAdsGuard implements CanActivate {
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<any>(null);
  private originalUrl: string | null = null;

  constructor(private http: HttpClient, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean | UrlTree> {
    // Store the original URL before any redirects
    this.originalUrl = this.router.url;

    return this.checkAuth().pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && !this.isRefreshing) {
          return this.handle401Error();
        }
        return of(this.redirectToLogin());
      })
    );
  }

  private checkAuth(): Observable<boolean | UrlTree> {
    return this.http
      .get(`${environment.apiUrl}Auth/is-auth`, {
        withCredentials: true,
        observe: 'response',
      })
      .pipe(  
        map((response) => {
          if (response.status === 200) {
            return true;
          }
          return this.redirectToLogin();
        })
      );
  }

  private handle401Error(): Observable<boolean | UrlTree> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.refreshToken().pipe(
        switchMap(() => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(true);
          // After successful refresh, try the auth check again
          return this.checkAuth();
        }),
        catchError(() => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(null);
          this.originalUrl = null; // Clear stored URL on error
          return of(this.redirectToLogin());
        })
      );
    }

    return this.refreshTokenSubject.pipe(
      filter((result) => result !== null),
      take(1),
      switchMap(() => this.checkAuth())
    );
  }

  private refreshToken(): Observable<any> {
    return new Observable((observer) => {
      const xhr = new XMLHttpRequest();
      xhr.open('POST', `${environment.apiUrl}Auth/refresh-token`, true);
      xhr.withCredentials = true;

      xhr.onload = () => {
        if (xhr.status === 200) {
          observer.next(xhr.response);
          observer.complete();
          // If we have a stored URL and we're coming from a refresh, redirect to it
          if (this.originalUrl && this.originalUrl !== '/identity/login') {
            const url = this.originalUrl;
            this.originalUrl = null; // Clear the stored URL
            this.router.createUrlTree([url]);
          }
        } else {
          observer.error(
            new HttpErrorResponse({
              status: xhr.status,
              statusText: xhr.statusText,
              error: xhr.response,
            })
          );
        }
      };

      xhr.onerror = () => {
        observer.error(
          new HttpErrorResponse({
            status: 0,
            statusText: 'Network Error',
            error: 'Network error occurred',
          })
        );
      };

      xhr.send();
    });
  }

  private redirectToLogin(): UrlTree {
    // Store the current URL as returnUrl if it's not already the login page
    const returnUrl =
      this.router.url !== '/identity/login' ? this.router.url : undefined;
    return this.router.createUrlTree(['/identity/login'], {
      queryParams: { returnUrl },
    });
  }
}
