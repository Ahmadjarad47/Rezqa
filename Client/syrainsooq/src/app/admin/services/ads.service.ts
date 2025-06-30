import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Ad } from '../../models/ad.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AdsService {
  private apiUrl = `${environment.apiUrl}admin/Ads`;

  constructor(private http: HttpClient) {}

  getAds(
    page: number = 1,
    pageSize: number = 10,
    searchTerm?: string
  ): Observable<{
    items: Ad[];
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  }> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<{
      items: Ad[];
      totalCount: number;
      totalPages: number;
      hasNextPage: boolean;
      hasPreviousPage: boolean;
    }>(this.apiUrl, { params });
  }

  updateAd(updatedAd: Ad): Observable<Ad> {
    return this.http.put<Ad>(`${this.apiUrl}/update-Active-show?id=${updatedAd.id}`, null);
  }

  deleteAd(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/delete?id=${id}`);
  }

  getAdById(id: number): Observable<Ad> {
    return this.http.get<Ad>(`${this.apiUrl}/${id}`);
  }

  createAd(ad: Omit<Ad, 'id'>): Observable<Ad> {
    return this.http.post<Ad>(this.apiUrl, ad);
  }

  toggleAdStatus(id: number): Observable<Ad> {
    return this.http.put<Ad>(`${this.apiUrl}/update-Active-show?id=${id}`, null);
  }
}
