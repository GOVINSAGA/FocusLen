import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DailyUsageDto {
  date: string;
  totalSeconds: number;
}

export interface TopAppDto {
  appName: string;
  totalDurationSeconds: number;
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private readonly http = inject(HttpClient);
  // Defaulting to 5028 based on earlier .NET 10 inspection
  private readonly baseUrl = 'http://localhost:5028/api/analytics';

  getDailyUsage(daysBack: number = 7): Observable<DailyUsageDto[]> {
    return this.http.get<DailyUsageDto[]>(`${this.baseUrl}/daily?daysBack=${daysBack}`);
  }

  getTopApps(limit: number = 5): Observable<TopAppDto[]> {
    return this.http.get<TopAppDto[]>(`${this.baseUrl}/top-apps?limit=${limit}`);
  }
}
