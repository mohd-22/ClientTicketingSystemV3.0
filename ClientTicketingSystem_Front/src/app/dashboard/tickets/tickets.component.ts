import { Component, OnInit } from '@angular/core';

export interface Ticket {
  id: string;
  subject: string;
  category: string;
  customer: string;
  priority: 'High' | 'Medium' | 'Low';
  status: 'In Progress' | 'Open' | 'Resolved' | 'Closed';
  lastUpdated: string;
  customerAvatar: string;
}

export interface TicketStats {
  openNow: number;
  urgentPriority: number;
  slaBreachRisk: number;
  avgResolution: string;
}

@Component({
  selector: 'app-tickets',
  templateUrl: './tickets.component.html',
  styles: []
})
export class TicketsComponent implements OnInit {
  stats: TicketStats = {
    openNow: 42,
    urgentPriority: 12,
    slaBreachRisk: 8,
    avgResolution: '3.4h'
  };

  tickets: Ticket[] = [
    {
      id: 'TKT-8291',
      subject: 'Payment Processing Error',
      category: 'Billing • API Integration',
      customer: 'John Doe Enterprises',
      priority: 'High',
      status: 'In Progress',
      lastUpdated: '12 mins ago',
      customerAvatar: 'JD'
    },
    {
      id: 'TKT-8285',
      subject: 'User Invitation Not Sent',
      category: 'Platform • Users',
      customer: 'Modern Web Co',
      priority: 'Medium',
      status: 'Open',
      lastUpdated: '45 mins ago',
      customerAvatar: 'MW'
    },
    {
      id: 'TKT-8270',
      subject: 'Feature Request: Dark Mode',
      category: 'Product • Feedback',
      customer: 'Sarah Smith',
      priority: 'Low',
      status: 'Resolved',
      lastUpdated: '2 hours ago',
      customerAvatar: 'SS'
    },
    {
      id: 'TKT-8266',
      subject: 'Password Reset Loop',
      category: 'Security • Access',
      customer: 'Tech Alliance',
      priority: 'High',
      status: 'In Progress',
      lastUpdated: '3 hours ago',
      customerAvatar: 'TA'
    },
    {
      id: 'TKT-8250',
      subject: 'Database Connection Timeout',
      category: 'Infrastructure • Backend',
      customer: 'Cloud Corp',
      priority: 'High',
      status: 'Closed',
      lastUpdated: 'Yesterday',
      customerAvatar: 'CC'
    }
  ];

  searchQuery = '';
  currentPage = 1;
  itemsPerPage = 10;
  totalItems = 42;

  constructor() { }

  ngOnInit(): void {
  }

  get filteredTickets(): Ticket[] {
    if (!this.searchQuery) return this.tickets;
    const query = this.searchQuery.toLowerCase();
    return this.tickets.filter(t =>
      t.id.toLowerCase().includes(query) ||
      t.subject.toLowerCase().includes(query) ||
      t.customer.toLowerCase().includes(query)
    );
  }

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.itemsPerPage);
  }

  previousPage(): void {
    if (this.currentPage > 1) this.currentPage--;
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) this.currentPage++;
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) this.currentPage = page;
  }

  getPriorityColor(priority: string): string {
    return priority === 'High' ? 'danger' : priority === 'Medium' ? 'warning' : 'success';
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'In Progress':
        return '#FFA500';
      case 'Open':
        return '#00A8E8';
      case 'Resolved':
        return '#06A77D';
      case 'Closed':
        return '#999999';
      default:
        return '#000000';
    }
  }
}
