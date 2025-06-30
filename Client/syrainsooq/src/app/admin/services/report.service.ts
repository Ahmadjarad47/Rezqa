import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, EMPTY } from 'rxjs';
import { environment } from '../../../environments/environment';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private apiUrl = environment.apiUrl + 'admin/report';

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  getStatus(): Observable<any> {
    if (isPlatformBrowser(this.platformId)) {
      return this.http.get(`${this.apiUrl}/status`);
    }
    return EMPTY;
  }

  getHealth(): Observable<any> {
    if (isPlatformBrowser(this.platformId)) {
      return this.http.get(`${this.apiUrl}/health`);
    }
    return EMPTY;
  }

  getMetrics(): Observable<any> {
    if (isPlatformBrowser(this.platformId)) {
      return this.http.get(`${this.apiUrl}/metrics`);
    }
    return EMPTY;
  }

  clearCache(): Observable<any> {
    if (isPlatformBrowser(this.platformId)) {
      return this.http.post(`${this.apiUrl}/clear-cache`, {});
    }
    return EMPTY;
  }
}
