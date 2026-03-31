import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  email: string;
}

const TOKEN_KEY = 'focustrack_jwt';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiBase = 'http://localhost:5028/api/auth';

  constructor(private readonly http: HttpClient) {}

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiBase}/register`, payload).pipe(
      tap(res => this.storeToken(res.token))
    );
  }

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiBase}/login`, payload).pipe(
      tap(res => this.storeToken(res.token))
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  private storeToken(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
  }
}
