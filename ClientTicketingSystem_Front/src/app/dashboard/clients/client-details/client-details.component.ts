import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { formatDistanceToNow } from 'date-fns/formatDistanceToNow';
import { ToastrService } from 'ngx-toastr';
import { UserDto, UsersService, UpdateUserRequest } from '../../../shared/services/users.service';
import { TicketDto, TicketsService } from '../../../shared/services/tickets.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-client-details',
  templateUrl: './client-details.component.html',
  styles: []
})

export class ClientDetailsComponent implements OnInit {
  private readonly PhonePattern = /^(?:\+9627|07)\d{8}$/;
  client: UserDto | null = null;
  tickets: TicketDto[] = [];
  isLoading = false;
  isTicketsLoading = false;
  isUpdatingStatus = false;
  isEditing = false;
  errorMessage = '';
  ticketsErrorMessage = '';
  showEditModal = false;
  editModel: Partial<UpdateUserRequest> = {};
  editForm!: FormGroup;
  isSaving = false;
  showConfirmStatusModal = false;
  confirmStatusAction: 'activate' | 'deactivate' | null = null;
  getAttachmentUrl(path: string | undefined): string {
    if (!path) return 'assets/images/default-avatar.png';
    const cleanPath = path.replace(/\\/g, '/').replace(/^\/+/, '');
    const base = (environment.apiUrl || '').replace(/\/+$/, '');
    return `${base}/${cleanPath}`;
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private usersService: UsersService,
    private ticketsService: TicketsService,
    private toastr: ToastrService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage = 'Client not found.';
      return;
    }

    this.loadClient(id);
  }

  goBack(): void {
    this.router.navigate(['/dashboard/clients']);
  }

  editClient(): void {
    if (!this.client) return;

    this.editModel = {
      fullName: this.client.fullName,
      phoneNumber: this.client.phoneNumber,
      address: this.client.address,
      dateOfBirth: this.client.dateOfBirth ? new Date(this.client.dateOfBirth).toISOString().slice(0, 10) : '',
      gender: this.client.gender
    };

    this.editForm = this.fb.group({
      fullName: [this.editModel.fullName || '', [Validators.required, Validators.minLength(2)]],
      phoneNumber: [this.editModel.phoneNumber || '', [Validators.required, Validators.pattern(this.PhonePattern)]],
      dateOfBirth: [this.editModel.dateOfBirth || ''],
      gender: [this.editModel.gender ?? ''],
      address: [this.editModel.address || '']
    });

    this.showEditModal = true;
  }

  cancelEdit(): void {
    this.showEditModal = false;
    this.editModel = {};
  }

  openConfirmStatusModal(): void {
    if (!this.client) return;
    this.confirmStatusAction = this.client.isActive ? 'deactivate' : 'activate';
    this.showConfirmStatusModal = true;
  }

  cancelConfirmStatus(): void {
    this.showConfirmStatusModal = false;
    this.confirmStatusAction = null;
  }

  confirmStatusChange(): void {
    this.showConfirmStatusModal = false;
    this.performToggleStatus();
  }

  saveEdit(): void {
    if (!this.client || !this.editForm) return;
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      this.toastr.error('Please fix the validation errors before saving.', 'Validation Error');
      return;
    }

    this.isSaving = true;
    const values = this.editForm.value;
    const payload: Partial<UpdateUserRequest> = {
      fullName: values.fullName ?? '',
      phoneNumber: values.phoneNumber ?? '',
      address: values.address ?? '',
      dateOfBirth: values.dateOfBirth ?? '',
      gender: values.gender ?? this.client.gender
    };

    this.usersService.updateUser(this.client.id, payload)
      .pipe(finalize(() => this.isSaving = false))
      .subscribe({
        next: (res) => {
          if (res?.success) {
            this.toastr.success(res.message || 'Client updated successfully', 'Success');
            this.showEditModal = false;
            this.loadClient(this.client!.id, true);
            return;
          }

          this.toastr.error(res?.message || 'Failed to update client', 'Error');
        },
        error: (err) => {
          const errorMessage = err?.error?.message || err?.message || 'Failed to update client. Please try again.';
          this.toastr.error(errorMessage, 'Error');
        }
      });
  }

  toggleStatus(): void {
    this.openConfirmStatusModal();
  }

  private performToggleStatus(): void {
    if (!this.client || this.isUpdatingStatus) {
      return;
    }

    this.isUpdatingStatus = true;
    const request$ = this.client.isActive
      ? this.usersService.deactivateUser(this.client.id)
      : this.usersService.activateUser(this.client.id);

    request$.pipe(finalize(() => this.isUpdatingStatus = false)).subscribe({
      next: (response) => {
        if (response?.success) {
          this.toastr.success(response.message || 'Client status updated successfully', 'Success');
          this.loadClient(this.client!.id, true);
          return;
        }

        this.toastr.error(response?.message || 'Unable to update client status.', 'Error');
      },
      error: (err) => {
        const errorMessage = err?.error?.message || err?.message || 'Unable to update client status.';
        this.toastr.error(errorMessage, 'Error');
      }
    });
  }

  getRoleLabel(role: number | string): string {
    const map: Record<number, string> = {
      1: 'Manager',
      2: 'Employee',
      3: 'Client'
    };

    return typeof role === 'number' ? map[role] ?? 'User' : role || 'User';
  }

  getGenderLabel(gender: number | string | null | undefined): string {
    if (gender === null || gender === undefined || gender === '') {
      return 'Not specified';
    }

    const normalized = String(gender).trim().toLowerCase();

    if (normalized === '1' || normalized === 'male') {
      return 'Male';
    }

    if (normalized === '2' || normalized === 'female') {
      return 'Female';
    }

    return 'Not specified';
  }

  getInitials(): string {
    if (!this.client?.fullName) {
      return 'C';
    }

    return this.client.fullName
      .split(' ')
      .filter(Boolean)
      .map(part => part[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();
  }

  toLocalDate(dateStr?: string | null): Date | null {
    if (!dateStr) return null;
    const s = String(dateStr);
    if (s.includes('Z') || /[+-]\d{2}:?\d{2}$/.test(s)) {
      return new Date(s);
    }
    return new Date(s + 'Z');
  }

  formatTimeAgo(dateStr?: string | null): string {
    if (!dateStr) return 'Never';
    const d = this.toLocalDate(dateStr);
    if (!d || Number.isNaN(d.getTime())) return 'Never';
    try {
      return formatDistanceToNow(d, { addSuffix: true });
    } catch {
      return d.toLocaleString();
    }
  }

  getStatusText(): string {
    return this.client?.isActive ? 'Active' : 'Inactive';
  }

  getStatusActionText(): string {
    return this.client?.isActive ? 'Deactivate Client' : 'Activate Client';
  }

  getStatusActionClass(): string {
    return this.client?.isActive ? 'btn-danger' : 'btn-success';
  }

  private loadClient(id: string, showToastOnError: boolean = false): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.usersService.getUserById(id)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (client) => {
          this.client = client;
          if (!client) {
            this.errorMessage = 'Client not found.';
            if (showToastOnError) {
              this.toastr.error('Client not found.', 'Error');
            }
            return;
          }

          this.loadClientTickets(client.id);
        },
        error: (err) => {
          this.client = null;
          const errorMsg = 'Failed to load client details.';
          this.errorMessage = errorMsg;
          if (showToastOnError) {
            const detailedMsg = err?.error?.message || err?.message || errorMsg;
            this.toastr.error(detailedMsg, 'Error');
          }
        }
      });
  }

  private loadClientTickets(clientId: string): void {
    this.isTicketsLoading = true;
    this.ticketsErrorMessage = '';

    this.ticketsService.getAllTickets('', 'created-desc', undefined, 1, 20, clientId, undefined)
      .pipe(finalize(() => this.isTicketsLoading = false))
      .subscribe({
        next: (response) => {
          this.tickets = response.data ?? [];
        },
        error: () => {
          this.tickets = [];
          this.ticketsErrorMessage = 'Failed to load client tickets.';
        }
      });
  }

}
