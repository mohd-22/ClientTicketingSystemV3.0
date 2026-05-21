import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TicketsService, TicketDto } from 'src/app/shared/services/tickets.service';
import { finalize } from 'rxjs/operators';
@Component({
  selector: 'app-ticket-details',
  templateUrl: './ticket-details.component.html',
  styles: []
})
export class TicketDetailsComponent implements OnInit {
  ticketId: string = '';
  isLoading = false;
  errorMessage = '';
  priority: string = 'High';
  relatedAssets: Array<{ name: string; url?: string }> = [];
  estimatedResolution: string | null = null;
  ticket: (TicketDto & { description?: string; createdDate?: string }) | null = null;

  constructor(private activatedRoute: ActivatedRoute, private ticketsService: TicketsService) { }

  ngOnInit(): void {
    this.activatedRoute.paramMap.subscribe(params => {
      this.ticketId = params.get('id') ?? '';
      if (this.ticketId) {
        this.loadTicketDetails();
      }
    });
  }

  private loadTicketDetails(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.ticketsService.getTicketById(this.ticketId)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: t => {
          this.ticket = t;
         
        },
        error: err => {
          console.error('Failed to load ticket', err);
          this.errorMessage = 'Failed to load ticket details.';
        }
      });
  }

  markAsFixed(): void {
    if (!this.ticket) return;
    this.ticket.isFixed = true;
    this.ticket.status = 'Resolved';
  }

  getStatusBadgeClass(): string {
    const status = (this.ticket?.status || '').toLowerCase();
    if (status.includes('progress')) return 'bg-info';
    if (status.includes('resolved') || status.includes('fixed')) return 'bg-success';
    if (status.includes('new')) return 'bg-primary';
    if (status.includes('closed')) return 'bg-secondary';
    return 'bg-light text-dark border';
  }

  getStatusIcon(): string {
    const status = (this.ticket?.status || '').toLowerCase();
    if (status.includes('progress')) return 'bi-hourglass-split';
    if (status.includes('resolved') || status.includes('fixed')) return 'bi-check-circle-fill';
    if (status.includes('new')) return 'bi-exclamation-circle-fill';
    if (status.includes('closed')) return 'bi-x-circle-fill';
    return 'bi-circle';
  }
}
