import { Component, ElementRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth';
import { AnalyticsService, DailyUsageDto, TopAppDto } from '../../core/analytics/analytics';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly analyticsService = inject(AnalyticsService);
  private readonly router = inject(Router);

  @ViewChild('dailyChartCanvas') dailyChartCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('topAppsChartCanvas') topAppsChartCanvas!: ElementRef<HTMLCanvasElement>;

  private dailyChart?: Chart;
  private topAppsChart?: Chart;

  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  dailyStats = signal<DailyUsageDto[]>([]);
  topAppsStats = signal<TopAppDto[]>([]);

  ngOnInit() {
    this.fetchAnalytics();
  }

  fetchAnalytics() {
    this.isLoading.set(true);
    // Fetch both datasets concurrently
    Promise.all([
      new Promise(resolve => this.analyticsService.getDailyUsage(7).subscribe({
        next: data => { this.dailyStats.set(data); resolve(true); },
        error: err => { console.error('Daily Usage Err:', err); resolve(false); }
      })),
      new Promise(resolve => this.analyticsService.getTopApps(5).subscribe({
        next: data => { this.topAppsStats.set(data); resolve(true); },
        error: err => { console.error('Top Apps Err:', err); resolve(false); }
      }))
    ]).then(results => {
      this.isLoading.set(false);
      if (results.every(r => r === true)) {
        setTimeout(() => this.renderCharts(), 0); // let UI update canvas refs
      } else {
        this.errorMessage.set('Failed to load some analytics data.');
      }
    });
  }

  renderCharts() {
    this.renderDailyChart();
    this.renderTopAppsChart();
  }

  renderDailyChart() {
    if (this.dailyChart) this.dailyChart.destroy();
    
    const ctx = this.dailyChartCanvas?.nativeElement.getContext('2d');
    if (!ctx) return;

    const data = this.dailyStats();
    this.dailyChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: data.map(d => {
          const dt = new Date(d.date);
          return dt.toLocaleDateString(undefined, { weekday: 'short' });
        }),
        datasets: [{
          label: 'Total Active Screening Time (minutes)',
          data: data.map(d => Math.floor(d.totalSeconds / 60)),
          backgroundColor: 'rgba(108, 99, 255, 0.7)',
          borderColor: 'rgba(108, 99, 255, 1)',
          borderWidth: 1,
          borderRadius: 4
        }]
      },
      options: { 
        responsive: true, 
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false }
        },
        scales: {
          y: { 
            beginAtZero: true, 
            grid: { color: 'rgba(255, 255, 255, 0.05)' } 
          },
          x: { 
            grid: { display: false } 
          }
        }
      }
    });
  }

  renderTopAppsChart() {
    if (this.topAppsChart) this.topAppsChart.destroy();

    const ctx = this.topAppsChartCanvas?.nativeElement.getContext('2d');
    if (!ctx) return;

    const data = this.topAppsStats();
    this.topAppsChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: data.map(d => d.appName),
        datasets: [{
          data: data.map(d => Math.floor(d.totalDurationSeconds / 60)),
          backgroundColor: [
            '#6c63ff', '#4ecca3', '#ff5757', '#fbbf24', '#3b82f6'
          ],
          borderWidth: 0,
          hoverOffset: 4
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '70%',
        plugins: {
          legend: { 
            position: 'bottom',
            labels: { color: '#e8eaf0' } 
          }
        }
      }
    });
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
