import { Routes } from '@angular/router';
import { PatchComponent } from './components/patch/patch';
import { CustomerComponent } from './components/customer/customer';
import { StatusComponent } from './components/status/status.component';

export const routes: Routes = [
  { path: 'patches', component: PatchComponent },
  { path: 'customers', component: CustomerComponent },
  { path: 'status', component: StatusComponent },
  { path: '', redirectTo: 'patches', pathMatch: 'full' },
];


