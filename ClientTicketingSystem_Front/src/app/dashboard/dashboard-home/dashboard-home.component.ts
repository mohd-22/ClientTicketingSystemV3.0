import { Component, OnInit } from '@angular/core';
import { TicketsService } from 'src/app/shared/services/tickets.service';
import { UsersService } from 'src/app/shared/services/users.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { finalize } from 'rxjs';
import { ChartData, ChartType } from 'chart.js';

@Component({
  selector: 'app-dashboard-home',
  templateUrl: './dashboard-home.component.html',
  styles: []
})
export class DashboardHomeComponent implements OnInit {
  stats = {
    lifetimeTickets: 0,
    openTickets: 0,
    closedTickets: 0,
    employees: 0,
    clients: 0
  };

  isLoading = false;
  isManager = false;

  public statusLabels: string[] = ['New', 'Assigned', 'InProgress', 'Closed'];
  public statusData: ChartData<'doughnut', number[], string | string[]> = {
    labels: this.statusLabels,
    datasets: [{ data: [0, 0, 0, 0], backgroundColor: ['#d9534f', '#f0ad4e', '#0275d8', '#5cb85c'] }]
  };
  public statusChartType: ChartType = 'doughnut';
  public statusChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'right' } }
  } as any;

  public statusColors: string[] = ['#d9534f', '#f0ad4e', '#0275d8', '#5cb85c'];

  public getDatasetValue(i: number): number {
    try {
      const v = (this.statusData && this.statusData.datasets && this.statusData.datasets[0] && (this.statusData.datasets[0].data as any)) ? (this.statusData.datasets[0].data as any)[i] : 0;
      return Number(v) || 0;
    } catch {
      return 0;
    }
  }

  public getProductValue(i: number): number {
    try {
      const d = this.productData && this.productData.datasets && this.productData.datasets[0] && (this.productData.datasets[0].data as any);
      const v = d ? (d[i] ?? 0) : 0;
      return Number(v) || 0;
    } catch {
      return 0;
    }
  }

  public productLabels: string[] = [];
  public productData: ChartData<'bar', number[], string | string[]> = { labels: [], datasets: [{ data: [], backgroundColor: [] }] };
  public productOptions = {
    indexAxis: 'y',
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        beginAtZero: true,
        grid: {
          display: false
        }
      },
      y: {
        grid: {
          color: '#f0f0f0'
        }
      }
    },
    plugins: { legend: { display: false } }
  } as any;

  public productColors: string[] = [
    'rgba(54, 162, 235, 0.7)',
    'rgba(54, 162, 235, 0.55)',
    'rgba(54, 162, 235, 0.45)',
    'rgba(54, 162, 235, 0.35)',
    'rgba(54, 162, 235, 0.28)',
    'rgba(54, 162, 235, 0.22)',
    'rgba(54, 162, 235, 0.18)'
  ];

  private updateProductChart(data: any[]): void {
    const counts: Record<string, number> = {};
    data.forEach(t => {
      const p = String(t.productName ?? t.productName ?? 'Unknown').trim() || 'Unknown';
      counts[p] = (counts[p] || 0) + 1;
    });

    const entries = Object.entries(counts).sort((a, b) => b[1] - a[1]);
    this.productLabels = entries.map(e => e[0]);
    const values = entries.map(e => e[1]);
    const bg = entries.map((_, i) => this.productColors[i % this.productColors.length]);

    this.productData = {
      labels: this.productLabels,
      datasets: [{
        data: values,
        backgroundColor: bg,
        hoverBackgroundColor: 'rgba(54, 162, 235, 1)',
        barPercentage: 0.4,
        categoryPercentage: 0.6,
        borderRadius: 6,
        borderSkipped: false
      }]
    };
  }

  constructor(
    private ticketsService: TicketsService,
    private usersService: UsersService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.isManager = this.authService.isManager();

    if (!this.isManager) {
      return;
    }

    this.loadStats();
  }

  private loadStats(): void {
    this.isLoading = true;

    this.ticketsService.getAllTickets(undefined, undefined, undefined, 1, 1).pipe(
      finalize(() => {
      })
    ).subscribe({
      next: resp => {
        const total = resp.count ?? 0;
        this.stats.lifetimeTickets = total;

        const pageSize = Math.max(1, total);
        this.ticketsService.getAllTickets(undefined, undefined, undefined, 1, pageSize).pipe(
          finalize(() => this.isLoading = false)
        ).subscribe({
          next: allResp => {
            const data = allResp.data ?? [];
            this.stats.openTickets = data.filter((t: any) => String(t.status).toLowerCase() !== 'closed').length;
            this.stats.closedTickets = data.filter((t: any) => String(t.status).toLowerCase() === 'closed' || t.isFixed === true).length;

            const counts: any = { New: 0, Assigned: 0, InProgress: 0, Closed: 0 };
            data.forEach((t: any) => {
              const s = String(t.status ?? '').toLowerCase();
              if (s.includes('new')) counts.New++;
              else if (s.includes('assigned')) counts.Assigned++;
              else if (s.includes('inprogress') || s.includes('in progress') || s.includes('in-progress')) counts.InProgress++;
              else if (s.includes('closed') || t.isFixed === true) counts.Closed++;
              else {
                if (t.isFixed === true) counts.Closed++;
                else counts.New++;
              }
            });

            this.statusData = {
              labels: this.statusLabels,
              datasets: [{ data: [counts.New, counts.Assigned, counts.InProgress, counts.Closed], backgroundColor: this.statusColors }]
            };

            this.updateProductChart(data);
          },
          error: err => {
            console.error('Failed to load full tickets for stats', err);
            this.isLoading = false;
          }
        });
      },
      error: err => {
        console.error('Failed to load tickets count', err);
        this.isLoading = false;
      }
    });

    this.usersService.getAllUsers(undefined, undefined, 'Employee', true, 1, 1).subscribe({
      next: r => this.stats.employees = r.count ?? 0,
      error: e => console.error('Failed to load employees count', e)
    });

    this.usersService.getAllUsers(undefined, undefined, 'Client', true, 1, 1).subscribe({
      next: r => this.stats.clients = r.count ?? 0,
      error: e => console.error('Failed to load clients count', e)
    });
  }
}
