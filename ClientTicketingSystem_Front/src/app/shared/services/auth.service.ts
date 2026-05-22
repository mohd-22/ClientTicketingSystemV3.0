import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { jwtDecode } from 'jwt-decode';

export interface RegisterRequest {
  fullName: string;
  userName: string;
  email: string;
  phoneNumber: string;
  address: string;
  dateOfBirth: string;
  gender: number;
  password: string;
  confirmPassword: string;
  imageUrl?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[] | null;
  statusCode: number;
}

export interface LoginRequest {
  emailOrUsername: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly registerUrl = `${environment.apiUrl}/api/Auth/Register`;
  private readonly loginUrl = `${environment.apiUrl}/api/Auth/Login`;

  constructor(private http: HttpClient) { }

  register(body: RegisterRequest): Observable<ApiResponse<RegisterRequest>> {
    return this.http.post<ApiResponse<RegisterRequest>>(this.registerUrl, body);
  }

  login(body: LoginRequest): Observable<string> {
    return this.http.post(this.loginUrl, body, { responseType: 'text' });
  }

  getUserRole(): string {
    const token = localStorage.getItem('access_token');
    if (!token) return '';

    try {
      const decoded: any = jwtDecode(token);
      return decoded.role || decoded.Role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '';
    } catch {
      return '';
    }
  }

  isClient(): boolean {
    return this.getUserRole().toLowerCase() === 'client';
  }
  isEmployee(): boolean {
    return this.getUserRole().toLowerCase() === 'employee';
  }
  isManager(): boolean {
    return this.getUserRole().toLowerCase() === 'manager';
  }

  getFullName(): string {
    const token = localStorage.getItem('access_token');
    if (!token) return '';

    try {
      const decoded: any = jwtDecode(token);
      return decoded.FullName || decoded.fullName || '';
    } catch {
      return '';
    }
  }

  getUserId(): string {
    const token = localStorage.getItem('access_token');
    if (!token) return '';

    try {
      const decoded: any = jwtDecode(token);
      return decoded.id || decoded.Id || decoded.sub || '';
    } catch {
      return '';
    }
  }

  logout(): void {
    localStorage.removeItem('access_token');
  }
}
