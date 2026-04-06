import { Routes } from '@angular/router';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { OverviewComponent } from './features/dashboard/overview/overview.component';
import { PreferencesComponent } from './features/settings/preferences/preferences.component';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'dashboard', component: OverviewComponent },
  { path: 'settings', component: PreferencesComponent },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' }
];
