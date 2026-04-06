import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AppLimitDto {
  id: string;
  appName: string;
  dailyLimitMinutes: number;
}

export interface LimitAlertDto {
  appName: string;
  dailyLimitMinutes: number;
  todayUsageMinutes: number;
  isExceeded: boolean;
}

export interface SetLimitDto {
  appName: string;
  dailyLimitMinutes: number;
}

@Injectable({ providedIn: 'root' })
export class LimitsService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = 'http://localhost:5028/api/limits';

  getAlerts(): Observable<LimitAlertDto[]> {
    return this.http.get<LimitAlertDto[]>(`${this.apiBase}/alerts`);
  }

  getLimits(): Observable<AppLimitDto[]> {
    return this.http.get<AppLimitDto[]>(this.apiBase);
  }

  setLimit(dto: SetLimitDto): Observable<AppLimitDto> {
    return this.http.post<AppLimitDto>(this.apiBase, dto);
  }

  deleteLimit(appName: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBase}/${encodeURIComponent(appName)}`);
  }
}
