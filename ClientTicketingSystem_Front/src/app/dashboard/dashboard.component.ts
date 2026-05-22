import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../shared/services/auth.service';

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
  constructor(private authService: AuthService, private router: Router) { }

  ngOnInit(): void {
    this.FullName = this.authService.getFullName();
    this.UserRole = this.authService.getUserRole();
    this.isManager = this.authService.isManager();
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

}
