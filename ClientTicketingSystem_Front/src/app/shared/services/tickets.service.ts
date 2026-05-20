import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './auth.service';

export interface CreateTicketRequest {
  title: string;
  description: string;
  productId: string;
}

export interface TicketDto {
  id: string;
  Id?: string;
  title: string;
  description?: string;
  clientName: string;
  assignedEmpName: string;
  productName: string;
  status: string;
  isFixed: boolean;
}

export interface PaginationDto<T> {
  pageIndex: number;
  pageSize: number;
  count: number;
  data: T[];
}

// Note: backend GetTicketById returns `TicketDto` (no Id/CreatedDate fields).

@Injectable({
  providedIn: 'root'
})
export class TicketsService {
  private readonly ticketsUrl = `${environment.apiUrl}/api/Tickets/GetAllTickets`;

  private readonly ticketByIdUrlBase = `${environment.apiUrl}/api/Tickets/GetTicketById`;

  private readonly createTicketUrl = `${environment.apiUrl}/api/Tickets/CreateTicket`;

  constructor(private http: HttpClient) { }

  getAllTickets(
    search?: string,
    sort?: string,
    status?: string,
    pageIndex: number = 1,
    pageSize: number = 10,
    productId?: string
  ): Observable<PaginationDto<TicketDto>> {
    let params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    if (sort && sort.trim()) {
      params = params.set('sort', sort.trim());
    }

    if (status && status.trim()) {
      params = params.set('status', status.trim());
    }

    if (productId && productId.trim()) {
      params = params.set('productId', productId.trim());
    }

    return this.http.get<ApiResponse<PaginationDto<any>>>(this.ticketsUrl, { params }).pipe(
      map(response => {
        const pag = response.data ?? { pageIndex: 1, pageSize: 10, count: 0, data: [] };
        pag.data = (pag.data ?? []).map((ticket: any) => {
          const normalized = {
            id: ticket.id ?? ticket.Id ?? '',
            Id: ticket.Id ?? ticket.id ?? '',
            title: ticket.title ?? ticket.Title ?? '',
            clientName: ticket.clientName ?? ticket.ClientName ?? '',
            assignedEmpName: ticket.assignedEmpName ?? ticket.AssignedEmpName ?? '',
            productName: ticket.productName ?? ticket.ProductName ?? '',
            status: ticket.status ?? ticket.Status ?? '',
            isFixed: ticket.isFixed ?? ticket.IsFixed ?? false
          } as TicketDto & { Id?: string };
          return normalized;
        });
        return pag as PaginationDto<TicketDto>;
      })
    );
  }

  getTicketById(id: string) {
    const url = `${this.ticketByIdUrlBase}/${id}`;
    return this.http.get<ApiResponse<any>>(url).pipe(
      map(response => {
        const t = response.data ?? {};
        const dto: TicketDto & { Id?: string } = {
          id: id,
          Id: id,
          title: t.title ?? t.Title ?? '',
          description: t.description ?? t.Description ?? '',
          clientName: t.clientName ?? t.ClientName ?? '',
          assignedEmpName: t.assignedEmpName ?? t.AssignedEmpName ?? '',
          productName: t.productName ?? t.ProductName ?? '',
          status: t.status ?? t.Status ?? '',
          isFixed: t.isFixed ?? t.IsFixed ?? false
        } as TicketDto & { Id?: string };
        return dto;
      })
    );
  }

  createTicket(body: CreateTicketRequest): Observable<ApiResponse<CreateTicketRequest>> {
    return this.http.post<ApiResponse<CreateTicketRequest>>(this.createTicketUrl, body);
  }
}
