import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CategoryDto, CreateCategoryRequest, UpdateCategoryRequest } from '../models/category.model';
import { PaginatedResponse, PaginationParams } from '../models/pagination.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private apiUrl = `${environment.apiUrl}admin/Category`;

  constructor(private http: HttpClient) {}

  getAll(params: PaginationParams): Observable<PaginatedResponse<CategoryDto>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());
    
    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    
    return this.http.get<PaginatedResponse<CategoryDto>>(this.apiUrl, { params: httpParams });
  }

  getById(id: string): Observable<CategoryDto> {
    return this.http.get<CategoryDto>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateCategoryRequest): Observable<string> {
    const formData = new FormData();
    formData.append('title', request.title);
    formData.append('image', request.image);
    formData.append('description', request.description);
    if (request.createdBy) {
      formData.append('createdBy', request.createdBy);
    }
    return this.http.post<string>(this.apiUrl, formData);
  }

  update(id: string, request: UpdateCategoryRequest): Observable<void> {
    const formData = new FormData();
    formData.append('id', request.id);
    formData.append('title', request.title);
    formData.append('description', request.description);
    if (request.image) {
      formData.append('image', request.image);
    }
    return this.http.put<void>(`${this.apiUrl}/${id}`, formData);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
