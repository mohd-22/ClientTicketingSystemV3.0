import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './auth.service';

export interface ProductDto {
  id: string;
  name: string;
  description: string;
  modules?: ModuleDto[];
  // backend may return ModulesCount (PascalCase) or modulesCount (camelCase)
  ModulesCount?: number;
  modulesCount?: number;
  createdat: string;
}

export interface ModuleDto {
  id: string;
  name: string;
  description: string;
  produtId: string;
}

export interface PaginationDto<T> {
  pageIndex: number;
  pageSize: number;
  count: number;
  data: T[];
}

@Injectable({
  providedIn: 'root'
})
export class ProductsService {
  private readonly productsUrl = `${environment.apiUrl}/api/Products/GetAllProducts`;

  constructor(private http: HttpClient) { }

  getAllProducts(search?: string, sort?: string, pageIndex: number = 1, pageSize: number = 10): Observable<PaginationDto<ProductDto>> {
    let params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    if (sort && sort.trim()) {
      params = params.set('sort', sort.trim());
    }

    return this.http.get<ApiResponse<PaginationDto<ProductDto>>>(this.productsUrl, { params }).pipe(
      map(response => response.data ?? { pageIndex: 1, pageSize: 10, count: 0, data: [] })
    );
  }

  getProductById(id: string): Observable<ProductDto | null> {
    const url = `${environment.apiUrl}/api/Products/GetProductById/${id}`;
    return this.http.get<ApiResponse<ProductDto>>(url).pipe(
      map(response => response.data ?? null)
    );
  }

  getAllModules(search?: string, sort?: string, productId?: string, pageIndex: number = 1, pageSize: number = 10): Observable<PaginationDto<ModuleDto>> {
    let params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    if (sort && sort.trim()) {
      params = params.set('sort', sort.trim());
    }

    if (productId && productId.trim()) {
      params = params.set('productId', productId.trim());
    }

    const url = `${environment.apiUrl}/api/ProductModules/GetAllModules`;
    return this.http.get<ApiResponse<PaginationDto<ModuleDto>>>(url, { params }).pipe(
      map(response => response.data ?? { pageIndex: 1, pageSize: 10, count: 0, data: [] })
    );
  }

  createModule(dto: { name: string; description: string; produtId: string }): Observable<boolean> {
    const url = `${environment.apiUrl}/api/ProductModules/CreateModule`;
    return this.http.post<ApiResponse<unknown>>(url, dto).pipe(
      map(() => true)
    );
  }

  updateModule(dto: { id: string; name: string; description: string }): Observable<boolean> {
    const url = `${environment.apiUrl}/api/ProductModules/UpdateModules`;
    return this.http.post<ApiResponse<unknown>>(url, dto).pipe(
      map(() => true)
    );
  }

  deleteModule(id: string): Observable<boolean> {
    const url = `${environment.apiUrl}/api/ProductModules/${id}`;
    return this.http.delete<ApiResponse<boolean>>(url).pipe(
      map(response => response.data ?? false)
    );
  }

  updateProduct(dto: { id: string; name: string; description: string }): Observable<ProductDto | null> {
    const url = `${environment.apiUrl}/api/Products/UpdateProduct`;
    return this.http.post<ApiResponse<ProductDto>>(url, dto).pipe(
      map(response => response.data ?? null)
    );
  }

  createProduct(dto: { name: string; description: string }): Observable<boolean> {
    const url = `${environment.apiUrl}/api/Products/CreateProduct`;
    return this.http.post<ApiResponse<unknown>>(url, dto).pipe(
      map(() => true)
    );
  }

  deleteProduct(id: string): Observable<boolean> {
    const url = `${environment.apiUrl}/api/Products/${id}`;
    return this.http.delete<ApiResponse<boolean>>(url).pipe(
      map(response => response.data ?? false)
    );
  }
}