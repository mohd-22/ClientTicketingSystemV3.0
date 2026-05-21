import { Component, OnInit } from '@angular/core';
import { finalize } from 'rxjs';
import { formatDistanceToNow } from 'date-fns/formatDistanceToNow';
import { UsersService, UserDto, PaginationDto } from '../../shared/services/users.service';

interface Client {
  id: string;
  name: string;
  email: string;
  role: string;
  status: string;
  lastActivity: string;
  avatar: string;
}

@Component({
    selector: 'app-clients',
    templateUrl: './clients.component.html',
    styles: []
})
  export class ClientsComponent implements OnInit {
    clients: Client[] = [];
    searchTerm: string = '';
    selectedRole: string = 'Client';
    selectedStatus: string = 'All Status';
    currentPage: number = 1;
    itemsPerPage: number = 4;
    totalCount: number = 0;
    isLoading: boolean = false;
    errorMessage: string = '';
  
    sortColumn: 'name' | 'email' | 'role' | 'isActive' = 'name';
    sortDirection: 'asc' | 'desc' = 'asc';

    stats = {
      totalClients: 0,
      vipAccounts: 0,
      recentlyAdded: 0
    };

    statuses = ['All Status', 'Active', 'Inactive'];

    constructor(private usersService: UsersService) { }

    ngOnInit(): void {
      this.loadClients();
    }

    get totalPages(): number {
      return Math.ceil(this.totalCount / this.itemsPerPage);
    }

    sortBy(column: 'name' | 'email' | 'role' | 'isActive'): void {
      if (this.sortColumn === column) {
        this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
      } else {
        this.sortColumn = column;
        this.sortDirection = 'asc';
      }

      this.currentPage = 1;
      this.loadClients();
    }

    loadClients(): void {
      this.isLoading = true;
      this.errorMessage = '';

      const sortQuery = this.buildSortQuery();
      const roleFilter = this.selectedRole;
      const isActiveFilter = this.selectedStatus === 'All Status' ? undefined : (this.selectedStatus === 'Active');

      this.usersService.getAllUsers(
        this.searchTerm || undefined,
        sortQuery,
        roleFilter,
        isActiveFilter,
        this.currentPage,
        this.itemsPerPage  
      ).pipe(
        finalize(() => this.isLoading = false)
      ).subscribe({
        next: (response: PaginationDto<UserDto>) => {
          this.clients = response.data.map(user => this.mapUserDtoToClient(user));
          this.totalCount = response.count;
          this.stats.totalClients = response.count;
          this.stats.vipAccounts = response.data.filter(u => u.role === 1 || u.role === 'Manager').length;
        },
        error: (error) => {
          this.errorMessage = 'Failed to load clients. Please try again.';
          console.error('Error loading clients:', error);
          this.clients = [];
          this.totalCount = 0;
        }
      });
    }

    private buildSortQuery(): string {
      const columnMap: { [key: string]: string } = {
        'name': 'name',
        'email': 'email',
        'role': 'role',
        'isActive': 'isActive'
      };

      const column = columnMap[this.sortColumn] || 'name';
      return this.sortDirection === 'asc' ? `${column}-asc` : `${column}-desc`;
    }

    private mapUserDtoToClient(userDto: UserDto): Client {
      const initials = userDto.fullName
        .split(' ')
        .map(n => n[0])
        .join('')
        .toUpperCase();
    
      return {
        id: userDto.id,
        name: userDto.fullName,
        email: userDto.email,
        role: this.getRoleNameFromEnum(userDto.role),
        status: userDto.isActive ? 'Active' : 'Inactive',
        lastActivity: userDto.lastLoginDate ? this.getTimeAgo(this.parseDateToLocal(userDto.lastLoginDate)) : 'Never',
        avatar: userDto.imageUrl || `https://dummyimage.com/40x40/007bff/ffffff&text=${initials}`
      };
    }

    private parseDateToLocal(dateStr?: string | null): Date {
      if (!dateStr) {
        return new Date(NaN);
      }
      const s = String(dateStr);
      if (s.includes('Z') || /[+-]\d{2}:?\d{2}$/.test(s)) {
        return new Date(s);
      }

      return new Date(s + 'Z');
    }

    private getRoleNameFromEnum(role: number | string): string {
      const roleMap: { [key: number]: string } = {
        1: 'Manager',
        2: 'Employee',
        3: 'Client'
      };

      if (typeof role === 'number') {
        return roleMap[role] || 'User';
      }
      return role || 'User';
    }

    private getTimeAgo(date: Date): string {
      try {
        return formatDistanceToNow(date, { addSuffix: true });
      } catch (e) {
        return date.toLocaleDateString();
      }
    }

    onSearch(): void {
      this.currentPage = 1;
      this.loadClients();
    }

    onRoleChange(): void {
      this.currentPage = 1;
      this.loadClients();
    }

    onStatusChange(): void {
      this.currentPage = 1;
      this.loadClients();
    }

    addClient(): void {
      alert('Add Client functionality - to be implemented');
    }

    viewClient(client: Client): void {
      alert(`View client: ${client.name}`);
    }

    deleteClient(client: Client): void {
      if (confirm(`Are you sure you want to delete ${client.name}?`)) {
        this.clients = this.clients.filter(c => c.id !== client.id);
        this.loadClients();
      }
    }

    getRoleBadgeColor(role: string): string {
      switch (role) {
        case 'Manager':
          return 'primary';
        case 'Employee':
          return 'info';
        case 'Client':
          return 'warning';
        default:
          return 'secondary';
      }
    }

    getStatusIcon(status: string): string {
      return status === 'Active' ? 'bi-check-circle-fill' : 'bi-x-circle-fill';
    }

    goToPage(page: number): void {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
        this.loadClients();
      }
    }
  }
