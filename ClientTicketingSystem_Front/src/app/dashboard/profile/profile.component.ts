import { Component, OnInit } from '@angular/core';
import { finalize } from 'rxjs';
import { AuthService } from '../../shared/services/auth.service';
import { UsersService, UserDto } from '../../shared/services/users.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  user: UserDto | null = null;
  loading = false;
  error = '';
  initials = 'U';
  uploadingAvatar = false;
  readonly backendUrl = 'https://localhost:7100/';

  constructor(
    private authService: AuthService,
    private usersService: UsersService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    const id = this.authService.getUserId();
    if (!id) {
      this.user = {
        id: '',
        fullName: this.authService.getFullName() || '',
        userName: '',
        email: '',
        phoneNumber: '',
        address: '',
        dateOfBirth: '',
        gender: 0,
        role: this.authService.getUserRole() || '',
        isActive: true
      };
      this.initials = this.computeInitials(this.user.fullName);
      return;
    }
    
    this.loading = true;
    this.usersService.getUserById(id).subscribe({
      next: u => {
        this.user = u;
        this.loading = false;
        this.initials = this.computeInitials(this.user?.fullName ?? '');
      },
      error: err => {
        console.error('Failed to load profile', err);
        this.error = 'Failed to load profile';
        this.loading = false;
      }
    });
  }
  

  private computeInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.split(' ').filter(p => p && p.length);
    const initialsArr: string[] = [];
    for (let i = 0; i < parts.length; i++) {
      const p = parts[i];
      initialsArr.push(p.charAt(0));
      if (initialsArr.length >= 2) break;
    }
    return initialsArr.join('') || 'U';
  }



  getAttachmentUrl(path: string | undefined): string {
    if (!path) return 'assets/images/default-avatar.png'; 
    
    
    const cleanPath = path.replace(/\\/g, '/');
    
    return `${this.backendUrl}${cleanPath}`;
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.toastr.error('Please choose a valid image file.', 'Invalid File');
      input.value = '';
      return;
    }

    this.uploadingAvatar = true;
    this.usersService.changeAvatar(file)
      .pipe(finalize(() => {
        this.uploadingAvatar = false;
        input.value = '';
      }))
      .subscribe({
        next: response => {
          if (response?.success) {
            this.toastr.success(response.message || 'Avatar updated successfully', 'Success');
            this.reloadProfile();
            return;
          }

          this.toastr.error(response?.message || 'Failed to update avatar', 'Error');
        },
        error: err => {
          const message = err?.error?.message || err?.message || 'Failed to update avatar';
          this.toastr.error(message, 'Error');
        }
      });
  }

  private reloadProfile(): void {
    const id = this.authService.getUserId();
    if (!id) return;

    this.loading = true;
    this.usersService.getUserById(id).pipe(finalize(() => this.loading = false)).subscribe({
      next: u => {
        this.user = u;
        this.initials = this.computeInitials(this.user?.fullName ?? '');
      },
      error: err => {
        console.error('Failed to reload profile', err);
      }
    });
  }


  getRoleLabel(role: number | string): string {
    const map: Record<number, string> = {
      1: 'Manager',
      2: 'Employee',
      3: 'Client'
    };

    if (role === null || role === undefined || role === '') return 'User';

    if (typeof role === 'number') {
      return map[role] ?? 'User';
    }

    const trimmed = String(role).trim();

    if (/^\d+$/.test(trimmed)) {
      const n = parseInt(trimmed, 10);
      return map[n] ?? 'User';
    }

    if (trimmed.length) return trimmed;

    return 'User';
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
      // lightweight relative formatting
      const now = new Date();
      const diff = Math.floor((now.getTime() - d.getTime()) / 1000);
      if (diff < 60) return `${diff}s ago`;
      if (diff < 3600) return `${Math.floor(diff/60)}m ago`;
      if (diff < 86400) return `${Math.floor(diff/3600)}h ago`;
      return `${Math.floor(diff/86400)}d ago`;
    } catch {
      return d.toLocaleString();
    }
  }

  getStatusText(): string {
    return this.user?.isActive ? 'Active' : 'Inactive';
  }
}
