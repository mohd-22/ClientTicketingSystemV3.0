import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { ProductDto, ProductsService } from 'src/app/shared/services/products.service';
import { TicketsService, TicketDto, UpdateTicketRequest } from 'src/app/shared/services/tickets.service';
import { UsersService, UserDto } from 'src/app/shared/services/users.service';
import { AuthService } from 'src/app/shared/services/auth.service';
@Component({
  selector: 'app-ticket-details',
  templateUrl: './ticket-details.component.html',
  styles: []
})
export class TicketDetailsComponent implements OnInit {
  ticketId: string = '';
  isLoading = false;
  errorMessage = '';
  products: ProductDto[] = [];
  employees: UserDto[] = [];
  showEditModal = false;
  isClient = false;
  isManager = false;
  isEmployee = false;
  showAssignModal = false;
  editForm!: FormGroup;
  isSaving = false;
  editSubmitted = false;
  editErrorMessage = '';
  selectedEmployeeId = '';
  assignLoading = false;
  assignErrorMessage = '';
  priority: string = 'High';
  relatedAssets: Array<{ name: string; url?: string }> = [];
  estimatedResolution: string | null = null;
  ticket: (TicketDto & { description?: string; createdDate?: string; productId?: string; ProductId?: string }) | null = null;

  constructor(
    private activatedRoute: ActivatedRoute,
    private ticketsService: TicketsService,
    private productsService: ProductsService,
    private usersService: UsersService,
    private toastr: ToastrService,
    private authService: AuthService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.activatedRoute.paramMap.subscribe(params => {
      this.ticketId = params.get('id') ?? '';
      if (this.ticketId) {
        this.loadTicketDetails();
      }
    });
    this.isClient = this.authService.isClient();
    this.isManager = this.authService.isManager();
    this.isEmployee = this.authService.isEmployee();
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
          this.loadProducts();
          this.loadEmployees();
        },
       
        error: err => {
          console.error('Failed to load ticket', err);
          this.errorMessage = 'Failed to load ticket details.';
        }
      });
  }

  private loadProducts(): void {
    this.productsService.getAllProducts('', 'name-asc', 1, 1000)
      .subscribe({
        next: response => {
          this.products = response.data ?? [];
          this.syncEditFormWithTicket();
        },
        error: err => {
          console.error('Failed to load products', err);
          this.products = [];
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

  private getTicketProductId(): string {
    const rawProductId = this.ticket?.productId ?? this.ticket?.ProductId ?? '';
    if (rawProductId) {
      return rawProductId;
    }

    const matchedProduct = this.products.find(product => product.name === this.ticket?.productName);
    return matchedProduct?.id ?? '';
  }

  private syncEditFormWithTicket(): void {
    if (!this.ticket || !this.editForm) {
      return;
    }

    const productId = this.getTicketProductId();
    if (productId) {
      this.editForm.patchValue({ productId });
    }
  }

  openEditModal(): void {
    if (!this.ticket) {
      return;
    }

    this.editErrorMessage = '';
    this.editSubmitted = false;

    this.editForm = this.fb.group({
      title: [this.ticket.title || '', [Validators.required, Validators.minLength(3)]],
      description: [this.ticket.description || '', [Validators.required, Validators.minLength(10)]],
      productId: [this.getTicketProductId(), Validators.required]
    });

    this.showEditModal = true;

    if (!this.products.length) {
      this.loadProducts();
    }
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.editErrorMessage = '';
    this.editSubmitted = false;
  }

  onEditModalBackdropClick(): void {
    if (!this.isSaving) {
      this.closeEditModal();
    }
  }

  saveEdit(): void {
    if (!this.ticket || !this.editForm) {
      return;
    }

    this.editSubmitted = true;
    this.editErrorMessage = '';

    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      this.editErrorMessage = 'Please fix the validation errors before saving.';
      this.toastr.error(this.editErrorMessage, 'Validation Error');
      return;
    }

    this.isSaving = true;
    const values = this.editForm.value;
    const payload: UpdateTicketRequest = {
      title: (values.title ?? '').trim(),
      description: (values.description ?? '').trim(),
      productId: values.productId
    };

    this.ticketsService.updateTicket(this.ticketId, payload)
      .pipe(finalize(() => this.isSaving = false))
      .subscribe({
        next: response => {
          if (response?.success === false) {
            this.editErrorMessage = response?.message || 'Failed to update ticket.';
            this.toastr.error(this.editErrorMessage, 'Error');
            return;
          }

          this.toastr.success(response?.message || 'Ticket updated successfully.', 'Success');
          this.closeEditModal();
          this.loadTicketDetails();
        },
        error: err => {
          console.error('Failed to update ticket', err);
          this.editErrorMessage = err?.error?.message || 'Failed to update ticket.';
          this.toastr.error(this.editErrorMessage, 'Error');
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
