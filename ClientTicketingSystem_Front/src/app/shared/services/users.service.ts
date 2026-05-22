import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './auth.service';

export interface UserDto {
  id: string;
  fullName: string;
  userName: string;
  email: string;
  phoneNumber: string;
  address: string;
  dateOfBirth: string;
  gender: number | string;
  imageUrl?: string;
  role: number | string;  // Backend returns as number enum, but can be string
  isActive: boolean;
  createdAt?: string;
  lastLoginDate?: string;
}

export interface PaginationDto<T> {
  pageIndex: number;
  pageSize: number;
  count: number;
  data: T[];
}

export interface CreateEmployeeRequest {
  fullName: string;
  userName: string;
  email: string;
  phoneNumber: string;
  address: string;
  dateOfBirth: string;
  gender: number;
  password: string;
  confirmPassword: string;
}

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private readonly usersUrl = `${environment.apiUrl}/api/Users/GetAllUsers`;
  private readonly changeAvatarUrl = `${environment.apiUrl}/api/Users/ChangeAvatar`;
  private readonly addEmployeeUrl = `${environment.apiUrl}/api/Users/AddEmployee`;
  private readonly activateUserUrl = `${environment.apiUrl}/api/Users/Activate`;
  private readonly deactivateUserUrl = `${environment.apiUrl}/api/Users/Deactivate`;

  constructor(private http: HttpClient) { }

  getAllUsers(
    search?: string,
    sort?: string,
    role?: string,
    isActive?: boolean,
    pageIndex: number = 1,
    pageSize: number = 10
  ): Observable<PaginationDto<UserDto>> {
    let params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    if (sort && sort.trim()) {
      params = params.set('sort', sort.trim());
    }

    if (role && role.trim() && role !== 'All Roles') {
      params = params.set('role', role.trim());
    }

    if (isActive !== undefined && isActive !== null) {
      params = params.set('isActive', isActive.toString());
    }

    return this.http.get<ApiResponse<PaginationDto<UserDto>>>(this.usersUrl, { params }).pipe(
      map(response => {
        const pag = response.data ?? { pageIndex: 1, pageSize: 10, count: 0, data: [] } as PaginationDto<UserDto>;
        pag.data = (pag.data ?? []).map(u => ({
          ...u,
          lastLoginDate: (u as any).lastLoginDate ?? (u as any).LastLoginDate ?? (u as any).LastLogin ?? (u as any).lastLogin ?? null,
          createdAt: (u as any).createdAt ?? (u as any).CreatedAt ?? (u as any).created ?? null
        } as UserDto));

        return pag;
      })
    );
  }

  getUserById(id: string): Observable<UserDto | null> {
    const url = `${environment.apiUrl}/api/Users/GetUserById/${id}`;
    return this.http.get<ApiResponse<UserDto>>(url).pipe(
      map(response => {
        const u = response.data ?? null;
        if (!u) {
          return null;
        }

        const normalized: UserDto = {
          ...u,
          lastLoginDate: (u as any).lastLoginDate ?? (u as any).LastLoginDate ?? (u as any).LastLogin ?? (u as any).lastLogin ?? null,
          createdAt: (u as any).createdAt ?? (u as any).CreatedAt ?? (u as any).created ?? null
        } as UserDto;

        return normalized;
      })
    );
  }

  changeAvatar(file: File): Observable<ApiResponse<boolean>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ApiResponse<boolean>>(this.changeAvatarUrl, formData);
  }

  activateUser(id: string): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.activateUserUrl}/${id}`, {});
  }

  deactivateUser(id: string): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.deactivateUserUrl}/${id}`, {});
  }

  createEmployee(payload: CreateEmployeeRequest): Observable<ApiResponse<CreateEmployeeRequest>> {
    return this.http.post<ApiResponse<CreateEmployeeRequest>>(this.addEmployeeUrl, payload);
  }

  updateUser(id: string, payload: Partial<UpdateUserRequest>): Observable<ApiResponse<unknown>> {
    const url = `${environment.apiUrl}/api/Users/${id}`;
    return this.http.put<ApiResponse<unknown>>(url, payload);
  }
}

export interface UpdateUserRequest {
  fullName: string;
  phoneNumber: string;
  address: string;
  dateOfBirth: string; // ISO date
  gender: number | string;
}
