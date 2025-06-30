import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  DynamicField, 
  GetAllDynamicFieldsRequest, 
  PaginatedResult, 
  CreateDynamicFieldsRequest, 
  UpdateDynamicFieldRequest, 
  DeleteDynamicFieldRequest, 
  DynamicFieldDto 
} from '../models/dynamic-field';
import { environment } from '../../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class DynamicFieldService {
  private apiUrl = environment.apiUrl + 'admin/DynamicField';

  constructor(private http: HttpClient) { }

  // Get all dynamic fields with pagination, search, and filtering
  getDynamicFields(request: GetAllDynamicFieldsRequest): Observable<PaginatedResult<DynamicFieldDto>> {
    let params = new HttpParams()
      .set('PageNumber', request.pageNumber.toString())
      .set('PageSize', request.pageSize.toString());

    if (request.searchTerm) {
      params = params.set('SearchTerm', request.searchTerm);
    }
    if (request.type) {
      params = params.set('Type', request.type);
    }
    if (request.categoryId) {
      params = params.set('CategoryId', request.categoryId.toString());
    }
    if (request.subCategoryId) {
      params = params.set('SubCategoryId', request.subCategoryId.toString());
    }

    return this.http.get<PaginatedResult<DynamicFieldDto>>(this.apiUrl, { params });
  }

  // Get dynamic field by id
  getDynamicField(id: number): Observable<DynamicFieldDto> {
    return this.http.get<DynamicFieldDto>(`${this.apiUrl}/${id}`);
  }

  // Create dynamic fields in bulk
  createDynamicFields(request: CreateDynamicFieldsRequest): Observable<number[]> {
    return this.http.post<number[]>(this.apiUrl, request);
  }

  // Update dynamic field
  updateDynamicField(id: number, request: UpdateDynamicFieldRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  // Delete dynamic field
  deleteDynamicField(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
} 