import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LimitsService, LimitAlertDto, SetLimitDto } from '../../../core/limits/limits.service';
import { AuthService } from '../../../core/auth/auth';

@Component({
  selector: 'app-preferences',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './preferences.component.html',
})
export class PreferencesComponent implements OnInit {
  private readonly limitsService = inject(LimitsService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  alerts = signal<LimitAlertDto[]>([]);
  isLoading = signal(true);
  isSaving = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Form model
  newAppName = '';
  newLimitMinutes: number | null = null;

  ngOnInit() {
    this.loadAlerts();
  }

  loadAlerts() {
    this.isLoading.set(true);
    this.limitsService.getAlerts().subscribe({
      next: data => {
        this.alerts.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load app limits. Please try again.');
        this.isLoading.set(false);
      }
    });
  }

  saveLimit() {
    if (!this.newAppName.trim() || !this.newLimitMinutes || this.newLimitMinutes <= 0) {
      this.errorMessage.set('Please enter a valid app name and a limit greater than 0 minutes.');
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const dto: SetLimitDto = {
      appName: this.newAppName.trim(),
      dailyLimitMinutes: this.newLimitMinutes
    };

    this.limitsService.setLimit(dto).subscribe({
      next: () => {
        this.successMessage.set(`Limit saved for "${dto.appName}".`);
        this.newAppName = '';
        this.newLimitMinutes = null;
        this.isSaving.set(false);
        this.loadAlerts();
      },
      error: () => {
        this.errorMessage.set('Failed to save limit. Please try again.');
        this.isSaving.set(false);
      }
    });
  }

  deleteLimit(appName: string) {
    this.limitsService.deleteLimit(appName).subscribe({
      next: () => {
        this.successMessage.set(`Removed limit for "${appName}".`);
        this.loadAlerts();
      },
      error: () => {
        this.errorMessage.set(`Failed to remove limit for "${appName}".`);
      }
    });
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
