import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../shared/services/auth.service';
import { UsersService, UserDto } from '../shared/services/users.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styles: []
})
export class DashboardComponent implements OnInit {

  sidebarCollapsed = false;
  mobileMenuOpen = false;
  FullName : string = '';
  UserRole : string = '';
  isManager = false;
  userImageUrl?: string;

  constructor(private authService: AuthService, private router: Router, private usersService: UsersService) { }

  ngOnInit(): void {
    this.FullName = this.authService.getFullName();
    this.UserRole = this.authService.getUserRole();
    this.isManager = this.authService.isManager();
    const id = this.authService.getUserId();
    if (id) {
      this.usersService.getUserById(id).subscribe({
        next: u => {
          this.userImageUrl = u?.imageUrl || undefined;
        },
        error: () => {
          this.userImageUrl = undefined;
        }
      });
    }
  }

  getAttachmentUrl(path: string | undefined): string {
    if (!path) return 'assets/images/default-avatar.png';
    const cleanPath = path.replace(/\\/g, '/').replace(/^\/+/, '');
    const base = (environment.apiUrl || '').replace(/\/+$/, '');
    return `${base}/${cleanPath}`;
  }

  getInitials(name?: string): string {
    if (!name) return 'U';
    try {
      return name.split(' ').map(n => n.charAt(0)).join('').slice(0, 2).toUpperCase();
    } catch {
      return name.charAt(0).toUpperCase();
    }
  }

  logout(): void {
    this.authService.logout();
    void this.router.navigateByUrl('/user/login', { replaceUrl: true });
  }
  toggleDesktopSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  openMobileMenu(): void {
    this.mobileMenuOpen = true;
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
  }

  goToProfile(): void {
    void this.router.navigateByUrl('/dashboard/profile');
    this.closeMobileMenu();
  }

}
