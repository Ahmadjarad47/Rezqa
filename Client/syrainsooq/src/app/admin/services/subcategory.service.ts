import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, EMPTY } from 'rxjs';
import {
  SubCategory,
  GetAllSubCategoriesRequest,
  PaginatedResult,
  CreateSubCategoryRequest,
  UpdateSubCategoryRequest,
  DeleteSubCategoryRequest,
  SubCategoryDto,
} from '../models/subcategory';
import { environment } from '../../../environments/environment.development';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class SubCategoryService {
  private apiUrl = environment.apiUrl + 'admin/SubCategory';

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  // Get all sub-categories with pagination, search, and filtering
  getSubCategories(
    request: GetAllSubCategoriesRequest
  ): Observable<PaginatedResult<SubCategoryDto>> {
    if (isPlatformBrowser(this.platformId)) {
      let params = new HttpParams()
        .set('PageNumber', request.pageNumber.toString())
        .set('PageSize', request.pageSize.toString());

      if (request.searchTerm) {
        params = params.set('SearchTerm', request.searchTerm);
      }
      if (request.categoryId) {
        params = params.set('CategoryId', request.categoryId.toString());
      }

      return this.http.get<PaginatedResult<SubCategoryDto>>(this.apiUrl, {
        params,
      });
    }
    return EMPTY;
  }

  // Get sub-category by id
  getSubCategory(id: number): Observable<SubCategoryDto> {
    if (isPlatformBrowser(this.platformId)) {
      return this.http.get<SubCategoryDto>(`${this.apiUrl}/${id}`);
    }
    return EMPTY;
  }

  // Create new sub-category
  createSubCategory(
    request: CreateSubCategoryRequest
  ): Observable<SubCategoryDto> {
    if (isPlatformBrowser(this.platformId)) {
      return this.http.post<SubCategoryDto>(this.apiUrl, request);
    }
    return EMPTY;
  }

  // Update sub-category
  updateSubCategory(
    id: number,
    request: UpdateSubCategoryRequest
  ): Observable<SubCategoryDto> {
    if (isPlatformBrowser(this.platformId)) {
      return this.http.put<SubCategoryDto>(`${this.apiUrl}/${id}`, request);
    }
    return EMPTY;
  }

  // Delete sub-category
  deleteSubCategory(id: number): Observable<void> {
    if (isPlatformBrowser(this.platformId)) {
      return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
    return EMPTY;
  }

  // Get sub-categories by category ID
  getSubCategoriesByCategory(
    categoryId: number,
    request: GetAllSubCategoriesRequest
  ): Observable<PaginatedResult<SubCategoryDto>> {
    let params = new HttpParams()
      .set('PageNumber', request.pageNumber.toString())
      .set('PageSize', request.pageSize.toString());

    if (request.searchTerm) {
      params = params.set('SearchTerm', request.searchTerm);
    }

    return this.http.get<PaginatedResult<SubCategoryDto>>(
      `${this.apiUrl}/by-category/${categoryId}`,
      { params }
    );
  }
}
