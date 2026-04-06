import { Component, AfterViewInit, ViewChild, ElementRef, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService } from '../../../core/analytics/analytics';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-overview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './overview.component.html'
})
export class OverviewComponent implements AfterViewInit, OnDestroy {
  private analyticsService = inject(AnalyticsService);

  @ViewChild('usageChart') usageChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('appsChart') appsChartRef!: ElementRef<HTMLCanvasElement>;

  usageChartInstance: Chart | null = null;
  appsChartInstance: Chart | null = null;
  loading = true;
  hasActivity = false;
  private pendingRequests = 2;

  ngAfterViewInit() {
    this.loadAnalytics();
  }

  loadAnalytics() {
    this.analyticsService.getDailyUsage(7).subscribe({
      next: data => {
        const labels = data.map(d => d.date);
        const values = data.map(d => Math.round(d.totalSeconds / 60)); // convert to minutes

        this.hasActivity = values.some(v => v > 0);

        if (!this.hasActivity) {
          this.checkLoading();
          return;
        }

        if (this.usageChartInstance) {
          this.usageChartInstance.destroy();
        }

        this.usageChartInstance = new Chart(this.usageChartRef.nativeElement, {
          type: 'bar',
          data: {
            labels,
            datasets: [{
              label: 'Minutes Focused',
              data: values,
              backgroundColor: 'rgba(99, 102, 241, 0.85)', // Indigo 500
              hoverBackgroundColor: 'rgba(79, 70, 229, 1)', // Indigo 600
              borderRadius: 6,
              borderSkipped: false
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: { display: false },
              tooltip: {
                backgroundColor: 'rgba(17, 24, 39, 0.9)',
                titleFont: { size: 13, family: "'Inter', sans-serif" },
                bodyFont: { size: 14, family: "'Inter', sans-serif" },
                padding: 12,
                cornerRadius: 8,
                displayColors: false
              }
            },
            scales: {
              y: { 
                beginAtZero: true,
                grid: { color: 'rgba(243, 244, 246, 1)' },
                border: { display: false }
              },
              x: {
                grid: { display: false },
                border: { display: false }
              }
            },
            animation: {
              duration: 1500,
              easing: 'easeOutQuart'
            }
          }
        });
        this.checkLoading();
      },
      error: () => this.checkLoading()
    });

    this.analyticsService.getTopApps(5).subscribe({
      next: data => {
        const labels = data.map(d => d.appName);
        const values = data.map(d => Math.round(d.totalDurationSeconds / 60));

        if (!this.hasActivity || values.length === 0) {
          this.checkLoading();
          return;
        }

        if (this.appsChartInstance) {
          this.appsChartInstance.destroy();
        }

        this.appsChartInstance = new Chart(this.appsChartRef.nativeElement, {
          type: 'doughnut',
          data: {
            labels,
            datasets: [{
              data: values,
              backgroundColor: [
                '#4f46e5', // Indigo 600
                '#06b6d4', // Cyan 500
                '#10b981', // Emerald 500
                '#f59e0b', // Amber 500
                '#ef4444'  // Red 500
              ],
              borderWidth: 2,
              borderColor: '#ffffff',
              hoverOffset: 6
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: { 
                position: 'bottom',
                labels: {
                  usePointStyle: true,
                  padding: 20,
                  font: { family: "'Inter', sans-serif" }
                }
              },
              tooltip: {
                backgroundColor: 'rgba(17, 24, 39, 0.9)',
                padding: 12,
                cornerRadius: 8
              }
            },
            cutout: '72%',
            animation: {
              duration: 1500,
              easing: 'easeOutQuart'
            }
          }
        });
        this.checkLoading();
      },
      error: () => this.checkLoading()
    });
  }

  checkLoading() {
    this.pendingRequests--;
    if (this.pendingRequests <= 0) {
      this.loading = false;
    }
  }

  ngOnDestroy() {
    if (this.usageChartInstance) this.usageChartInstance.destroy();
    if (this.appsChartInstance) this.appsChartInstance.destroy();
  }
}
