import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './auth.service';

export interface AttachmentDto {
  id: string;
  fileName: string;
  filePath: string;
}

@Injectable({
  providedIn: 'root'
})
export class AttachmentsService {
  constructor(private http: HttpClient) { }

  getAttachmentsByTicket(ticketId: string): Observable<ApiResponse<AttachmentDto[]>> {
    return this.http.get<ApiResponse<AttachmentDto[]>>(`${environment.apiUrl}/api/Attachment/Ticket/${ticketId}`);
  }

  uploadAttachment(ticketId: string, file: File): Observable<ApiResponse<unknown>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ApiResponse<unknown>>(
      `${environment.apiUrl}/api/Attachment/UploadAttachment/${ticketId}`,
      formData
    );
  }
}
