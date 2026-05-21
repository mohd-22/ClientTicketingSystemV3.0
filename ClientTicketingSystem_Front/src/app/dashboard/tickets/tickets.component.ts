import { Component, OnInit } from '@angular/core';
import { finalize } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from 'src/app/shared/services/auth.service';
import { CreateTicketRequest, PaginationDto, TicketDto, TicketsService } from 'src/app/shared/services/tickets.service';
import { UsersService, UserDto } from 'src/app/shared/services/users.service';
import { PaginationDto as ProductsPaginationDto, ProductDto, ProductsService } from 'src/app/shared/services/products.service';

@Component({
  selector: 'app-tickets',
  templateUrl: './tickets.component.html',
  styles: []
})
export class TicketsComponent implements OnInit {
  tickets: TicketDto[] = [];
  products: ProductDto[] = [];
  clients: UserDto[] = [];
  employees: UserDto[] = [];
  selectedClient: string = '';
  selectedEmployee: string = '';

  searchQuery = '';
  selectedStatus = 'All Status';
  currentPage = 1;
  itemsPerPage = 10;
  totalItems = 0;
  isLoading = false;
  errorMessage = '';
  statusFilter: string = 'all';
  sortColumn: 'title' | 'clientName' | 'assignedEmpName' = 'title';
  sortDirection: 'asc' | 'desc' = 'asc';
  statuses = ['All Status', 'New', 'InProgress', 'Paused', 'Closed'];
  isClientUser = false;
  isEmployeeUser = false;
  isManagerUser = false;
  showCreateModal = false;
  createLoading = false;
  createErrorMessage = '';
  createSubmitted = false;
  createForm!: FormGroup;

  constructor(
    private ticketsService: TicketsService,
    private productsService: ProductsService,
    private authService: AuthService,
    private usersService: UsersService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.isClientUser = this.authService.isClient();
    this.isEmployeeUser = this.authService.isEmployee();
    this.isManagerUser = this.authService.isManager();
    this.createForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      productId: ['', Validators.required]
    });

    this.loadProducts();

    if (this.isManagerUser) {
      this.loadUsersForFilters();
    }

    this.loadTickets();
  }

  openCreateModal(): void {
    if (!this.isClientUser) {
      return;
    }

    this.createSubmitted = false;
    this.createErrorMessage = '';
    this.createForm.reset();
    if (this.products.length > 0) {
      this.createForm.patchValue({ productId: this.products[0].id });
    }
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.createSubmitted = false;
    this.createErrorMessage = '';
    this.createForm.reset();
  }

  onSearchChange(): void {
    this.currentPage = 1;
    this.loadTickets();
  }

  onStatusChange(): void {
    this.currentPage = 1;
    this.loadTickets();
  }

  onClientChange(): void {
    this.currentPage = 1;
    this.loadTickets();
  }

  onEmployeeChange(): void {
    this.currentPage = 1;
    this.loadTickets();
  }

  sortBy(column: 'title' | 'clientName' | 'assignedEmpName'): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }

    this.currentPage = 1;
    this.loadTickets();
  }

  private buildSortQuery(): string {
    const columnMap: Record<'title' | 'clientName' | 'assignedEmpName', string> = {
      title: 'title',
      clientName: 'client',
      assignedEmpName: 'assigned'
    };

    const column = columnMap[this.sortColumn] || 'title';
    return this.sortDirection === 'asc' ? `${column}-asc` : `${column}-desc`;
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

    const sortQuery = this.buildSortQuery();
    const status = this.selectedStatus === 'All Status' ? undefined : this.selectedStatus;

    this.ticketsService.getAllTickets(
      this.searchQuery,
      sortQuery,
      status,
      this.currentPage,
      this.itemsPerPage,
      this.selectedClient,
      this.selectedEmployee
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

  private loadProducts(): void {
    this.productsService.getAllProducts('', 'name-asc', 1, 1000)
      .subscribe({
        next: (response: ProductsPaginationDto<ProductDto>) => {
          this.products = response.data ?? [];
          if (this.products.length > 0 && !this.createForm.value.productId) {
            this.createForm.patchValue({ productId: this.products[0].id });
          }
        },
        error: () => {
          this.products = [];
          this.toastr.error('Unable to load products for ticket creation.', 'Error');
        }
      });
  }

    private loadUsersForFilters(): void {
      this.usersService.getAllUsers('', 'name-asc', 'Employee', undefined, 1, 1000)
        .subscribe({
          next: (resp) => this.employees = resp.data ?? [],
          error: () => this.employees = []
        });

      this.usersService.getAllUsers('', 'name-asc', 'Client', undefined, 1, 1000)
        .subscribe({
          next: (resp) => this.clients = resp.data ?? [],
          error: () => this.clients = []
        });
    }

  submitCreateTicket(): void {
    this.createSubmitted = true;
    this.createErrorMessage = '';

    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.createLoading = true;
    const payload: CreateTicketRequest = {
      title: (this.createForm.value.title ?? '').trim(),
      description: (this.createForm.value.description ?? '').trim(),
      productId: this.createForm.value.productId
    };

    this.ticketsService.createTicket(payload)
      .pipe(finalize(() => this.createLoading = false))
      .subscribe({
        next: (response) => {
          if (response?.success) {
            this.toastr.success(response.message || 'Ticket created successfully.', 'Success');
            this.closeCreateModal();
            this.currentPage = 1;
            this.loadTickets();
            return;
          }

          this.createErrorMessage = response?.message || 'Failed to create ticket.';
          this.toastr.error(this.createErrorMessage, 'Error');
        },
        error: (err) => {
          this.createErrorMessage = err?.error?.message || 'Failed to create ticket.';
          this.toastr.error(this.createErrorMessage, 'Error');
        }
      });
  }

  hasCreateFieldError(controlName: string): boolean {
    const control = this.createForm.get(controlName);
    return !!control && control.invalid && (control.touched || control.dirty || this.createSubmitted);
  }

  getCreateFieldErrorMessage(controlName: string): string {
    const control = this.createForm.get(controlName);

    if (!control || !control.errors) {
      return '';
    }

    if (control.errors['required']) {
      return `${this.getCreateFieldLabel(controlName)} is required.`;
    }

    if (control.errors['minlength']) {
      const requiredLength = control.errors['minlength'].requiredLength;
      return `${this.getCreateFieldLabel(controlName)} must be at least ${requiredLength} characters long.`;
    }

    return `${this.getCreateFieldLabel(controlName)} is invalid.`;
  }

  private getCreateFieldLabel(controlName: string): string {
    switch (controlName) {
      case 'title':
        return 'Title';
      case 'description':
        return 'Description';
      case 'productId':
        return 'Product';
      default:
        return 'Field';
    }
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
