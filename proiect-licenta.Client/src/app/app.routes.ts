import { Routes } from '@angular/router';
import {LoginComponent} from '../components/login/login.component';
import {RegisterComponent} from '../components/register/register.component';
import {HomeComponent} from '../components/home/home.component';
import {MetamaskLoginComponent} from '../components/metamask-login/metamask-login.component';
import {AppDetailsComponent} from '../components/app-details/app-details.component';
import {PaymentHistoryComponent} from '../components/payment-history/payment-history.component';
import {CreateAppComponent} from '../components/create-app/create-app.component';
import {RoleGuard} from '../guards/role.guard';
import {AdminRefundsComponent} from '../components/admin-refunds/admin-refunds.component';
import {UnauthorizedComponent} from '../components/unauthorized/unauthorized.component';
import {LibraryComponent} from '../components/library/library.component';
import {EditAppComponent} from '../components/edit-app/edit-app.component';

export const routes: Routes = [
  {path: 'login', component: LoginComponent},
  {path: 'register', component: RegisterComponent},
  {path: 'home', component: HomeComponent},
  {path: 'meta', component: MetamaskLoginComponent},
  {path: 'payment-history', component: PaymentHistoryComponent},

  {path: 'app/:id', component: AppDetailsComponent},
  {path: 'apps/create', component: CreateAppComponent,
    canActivate: [RoleGuard], data: {roles: ['DEVELOPER', 'ADMIN']}},
  {path: 'apps/edit/:id', component: EditAppComponent,
    canActivate: [RoleGuard], data: {roles: ['DEVELOPER', 'ADMIN']}},

  {path: 'refunds/admin', component: AdminRefundsComponent,
    canActivate: [RoleGuard], data: {roles: ['ADMIN']}},
  {path: 'library', component: LibraryComponent,
    canActivate: [RoleGuard], data: {roles: ['DEVELOPER', 'ADMIN', 'USER']}},
  {path: 'unauthorized', component: UnauthorizedComponent},
];
