import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { formatDistanceToNow } from 'date-fns/formatDistanceToNow';
import { ToastrService } from 'ngx-toastr';
import { UserDto, UsersService, UpdateUserRequest } from '../../../shared/services/users.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-user-details',
  templateUrl: './user-details.component.html',
  styleUrls: ['./user-details.component.css']
})
export class UserDetailsComponent implements OnInit {
  user: UserDto | null = null;
  isLoading = false;
  isUpdatingStatus = false;
  isEditing = false;
  errorMessage = '';
  showEditModal = false;
  editModel: Partial<UpdateUserRequest> = {};
  editForm!: FormGroup;
  isSaving = false;
  showConfirmStatusModal = false;
  confirmStatusAction: 'activate' | 'deactivate' | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private usersService: UsersService,
    private toastr: ToastrService
    ,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage = 'User not found.';
      return;
    }

    this.loadUser(id);
  }

  goBack(): void {
    this.router.navigate(['/dashboard/users']);
  }

  editUser(): void {
    if (!this.user) return;
    // populate edit model
    this.editModel = {
      fullName: this.user.fullName,
      phoneNumber: this.user.phoneNumber,
      address: this.user.address,
      dateOfBirth: this.user.dateOfBirth ? new Date(this.user.dateOfBirth).toISOString().slice(0,10) : '',
      gender: this.user.gender
    };
    // initialize reactive form with validation
    this.editForm = this.fb.group({
      fullName: [this.editModel.fullName || '', [Validators.required, Validators.minLength(2)]],
      phoneNumber: [this.editModel.phoneNumber || '', [Validators.pattern(/^\+?[0-9\s-]{7,15}$/)]],
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
    if (!this.user) return;
    this.confirmStatusAction = this.user.isActive ? 'deactivate' : 'activate';
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
    if (!this.user) return;
    if (!this.editForm) return;
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
      gender: values.gender ?? this.user.gender
    };

    this.usersService.updateUser(this.user.id, payload)
      .pipe(finalize(() => this.isSaving = false))
      .subscribe({
        next: (res) => {
          if (res?.success) {
            this.toastr.success(res.message || 'User updated successfully', 'Success');
            this.showEditModal = false;
            this.loadUser(this.user!.id, true);
            return;
          }
          this.toastr.error(res?.message || 'Failed to update user', 'Error');
        },
        error: (err) => {
          const errorMessage = err?.error?.message || err?.message || 'Failed to update user. Please try again.';
          this.toastr.error(errorMessage, 'Error');
        }
      });
  }

  toggleStatus(): void {
    this.openConfirmStatusModal();
  }

  private performToggleStatus(): void {
    if (!this.user || this.isUpdatingStatus) {
      return;
    }

    this.isUpdatingStatus = true;
    const request$ = this.user.isActive
      ? this.usersService.deactivateUser(this.user.id)
      : this.usersService.activateUser(this.user.id);

    request$.pipe(finalize(() => this.isUpdatingStatus = false)).subscribe({
      next: (response) => {
        if (response?.success) {
          this.toastr.success(response.message || 'User status updated successfully', 'Success');
          this.loadUser(this.user!.id, true);
          return;
        }

        this.toastr.error(response?.message || 'Unable to update user status.', 'Error');
      },
      error: (err) => {
        const errorMessage = err?.error?.message || err?.message || 'Unable to update user status.';
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
    if (!this.user?.fullName) {
      return 'U';
    }

    return this.user.fullName
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
    return this.user?.isActive ? 'Active' : 'Inactive';
  }

  getStatusActionText(): string {
    return this.user?.isActive ? 'Deactivate User' : 'Activate User';
  }

  getStatusActionClass(): string {
    return this.user?.isActive ? 'btn-danger' : 'btn-success';
  }

  private loadUser(id: string, showToastOnError: boolean = false): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.usersService.getUserById(id)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (user) => {
          this.user = user;
          if (!user) {
            this.errorMessage = 'User not found.';
            if (showToastOnError) {
              this.toastr.error('User not found.', 'Error');
            }
          }
        },
        error: (err) => {
          this.user = null;
          const errorMsg = 'Failed to load user details.';
          this.errorMessage = errorMsg;
          if (showToastOnError) {
            const detailedMsg = err?.error?.message || err?.message || errorMsg;
            this.toastr.error(detailedMsg, 'Error');
          }
        }
      });
  }

}
