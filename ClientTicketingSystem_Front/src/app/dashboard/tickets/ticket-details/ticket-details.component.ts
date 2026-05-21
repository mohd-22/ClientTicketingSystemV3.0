import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { TicketsService, TicketDto } from 'src/app/shared/services/tickets.service';
import { UsersService, UserDto } from 'src/app/shared/services/users.service';
@Component({
  selector: 'app-ticket-details',
  templateUrl: './ticket-details.component.html',
  styles: []
})
export class TicketDetailsComponent implements OnInit {
  ticketId: string = '';
  isLoading = false;
  errorMessage = '';
  employees: UserDto[] = [];
  showAssignModal = false;
  selectedEmployeeId = '';
  assignLoading = false;
  assignErrorMessage = '';
  priority: string = 'High';
  relatedAssets: Array<{ name: string; url?: string }> = [];
  estimatedResolution: string | null = null;
  ticket: (TicketDto & { description?: string; createdDate?: string }) | null = null;

  constructor(
    private activatedRoute: ActivatedRoute,
    private ticketsService: TicketsService,
    private usersService: UsersService,
    private toastr: ToastrService
  ) { }

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
          console.log('Ticket Status:', this.ticket?.status);
          this.loadEmployees();
        },
       
        error: err => {
          console.error('Failed to load ticket', err);
          this.errorMessage = 'Failed to load ticket details.';
        }
      });
  }

  private loadEmployees(): void {
    this.usersService.getAllUsers('', 'name-asc', 'Employee', true, 1, 1000)
      .subscribe({
        next: response => this.employees = response.data ?? [],
        error: err => {
          console.error('Failed to load employees', err);
          this.employees = [];
        }
      });
  }

  openAssignModal(): void {
    if (!this.ticket) {
      return;
    }

    this.assignErrorMessage = '';
    this.selectedEmployeeId = '';
    this.showAssignModal = true;
    if (!this.employees.length) {
      this.loadEmployees();
    }
  }

  closeAssignModal(): void {
    this.showAssignModal = false;
    this.assignErrorMessage = '';
    this.selectedEmployeeId = '';
  }

  onAssignModalBackdropClick(): void {
    if (!this.assignLoading) {
      this.closeAssignModal();
    }
  }

  assignTicket(): void {
    if (!this.ticket || !this.selectedEmployeeId) {
      this.assignErrorMessage = 'Please choose an employee to assign.';
      return;
    }

    this.assignLoading = true;
    this.assignErrorMessage = '';

    this.ticketsService.assignTicketToEmployee(this.ticketId, this.selectedEmployeeId)
      .pipe(finalize(() => this.assignLoading = false))
      .subscribe({
        next: response => {
          if (response?.success === false) {
            this.assignErrorMessage = response?.message || 'Failed to assign ticket.';
            this.toastr.error(this.assignErrorMessage, 'Error');
            return;
          }

          this.toastr.success(response?.message || 'Ticket assigned successfully.', 'Success');
          this.closeAssignModal();
          this.loadTicketDetails();
        },
        error: err => {
          console.error('Failed to assign ticket', err);
          this.assignErrorMessage = err?.error?.message || 'Failed to assign ticket.';
          this.toastr.error(this.assignErrorMessage, 'Error');
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
