import { Component, OnInit } from '@angular/core';
import { finalize } from 'rxjs';
import { PaginationDto, TicketDto, TicketsService } from 'src/app/shared/services/tickets.service';

@Component({
  selector: 'app-tickets',
  templateUrl: './tickets.component.html',
  styles: []
})
export class TicketsComponent implements OnInit {
  tickets: TicketDto[] = [];

  searchQuery = '';
  currentPage = 1;
  itemsPerPage = 10;
  totalItems = 0;
  isLoading = false;
  errorMessage = '';
  sortQuery = 'title-asc';
  statusFilter: string = 'all';

  constructor(private ticketsService: TicketsService) { }

  ngOnInit(): void {
    this.loadTickets();
  }

  onSearchChange(): void {
    this.currentPage = 1;
    this.loadTickets();
  }

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.itemsPerPage);
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadTickets();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadTickets();
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadTickets();
    }
  }

  private loadTickets(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const status = this.statusFilter === 'all' ? undefined : this.statusFilter;

    this.ticketsService.getAllTickets(
      this.searchQuery,
      this.sortQuery,
      status,
      this.currentPage,
      this.itemsPerPage
    ).pipe(
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: (response: PaginationDto<TicketDto>) => {
        this.tickets = response.data ?? [];
        this.totalItems = response.count ?? 0;
      },
      error: () => {
        this.tickets = [];
        this.totalItems = 0;
        this.errorMessage = 'Unable to load tickets from the API.';
      }
    });
  }

  getStatusBadge(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'new':
        return 'bg-primary';
      case 'inprogress':
      case 'in progress':
        return 'bg-warning text-dark';
      case 'resolved':
      case 'fixed':
        return 'bg-success';
      case 'closed':
        return 'bg-secondary';
      default:
        return 'bg-light text-dark border';
    }
  }
}
