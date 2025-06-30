import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { Observable } from 'rxjs';

export interface ComponentCanDeactivate {
  canDeactivate: () => boolean | Observable<boolean>;
}

@Injectable({
  providedIn: 'root'
})
export class AdsLeaveGuard implements CanDeactivate<ComponentCanDeactivate> {
  canDeactivate(
    component: ComponentCanDeactivate
  ): boolean | Observable<boolean> {
    return new Observable<boolean>(observer => {
      // const result = window.confirm('Are you sure you want to leave? Any unsaved changes will be lost.');
     setTimeout(()=>{

     },10000)
      observer.next(true);
      observer.complete();
    });
  }
}
