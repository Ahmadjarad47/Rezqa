import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { delay, finalize } from 'rxjs';

export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  const loader = inject(NgxSpinnerService);
  req = req.clone({
    withCredentials: true,
  });
  if (
    !req.url.includes('/admin') &&
    !req.url.includes('/GetAdLists') &&
    !req.url.includes('/AdHome')
  ) {
    loader.show();
  }
  return next(req).pipe(
    delay(1000),
    finalize(() => {
      loader.hide();
    })
  );
};
