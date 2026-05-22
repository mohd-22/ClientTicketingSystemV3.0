import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './auth.service';

export interface CreateCommentRequest {
  text: string;
  ticketId: string;
}

export interface CommentReadDto {
  id: string;
  text: string;
  createdAt: string;
  userName: string;
  userRole: string;
  userId: string;
}

@Injectable({ providedIn: 'root' })
export class CommentsService {
  private readonly baseUrl = `${environment.apiUrl}/api/Comments`;

  constructor(private http: HttpClient) { }

  getComments(ticketId: string): Observable<ApiResponse<CommentReadDto[]>> {
    const url = `${this.baseUrl}/${ticketId}`;
    return this.http.get<ApiResponse<CommentReadDto[]>>(url);
  }

  createComment(body: CreateCommentRequest) {
    return this.http.post<ApiResponse<CreateCommentRequest>>(this.baseUrl, body);
  }
}
