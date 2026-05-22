import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { LoginComponent } from './user/login/login.component';
import { UserComponent } from './user/user.component';
import { RegistrationComponent } from './user/registration/registration.component';
import { AuthGuard } from './guards/auth.guard';
import { GuestGuardGuard } from './guards/guest-guard.guard';
import { TicketsComponent } from './dashboard/tickets/tickets.component';
import { DashboardHomeComponent } from './dashboard/dashboard-home/dashboard-home.component';
import { ProductsComponent } from './dashboard/products/products.component';
import { UsersComponent } from './dashboard/users/users.component';
import { CreateEmployeeComponent } from './dashboard/users/create-employee/create-employee.component';
import { UserDetailsComponent } from './dashboard/users/user-details/user-details.component';
import { ClientsComponent } from './dashboard/clients/clients.component';
import { ClientDetailsComponent } from './dashboard/clients/client-details/client-details.component';
import { TicketDetailsComponent } from './dashboard/tickets/ticket-details/ticket-details.component';
import { ManagerGuard } from './guards/manager.guard';
import { ProfileComponent } from './dashboard/profile/profile.component';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'user/login' },
  {
    path: 'user',
    component: UserComponent,
    children: [
      { path: 'signup', component: RegistrationComponent,canActivate: [GuestGuardGuard] },
      { path: 'login', component: LoginComponent, canActivate: [GuestGuardGuard] },
    ]
  },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard], children: [
    { path: '',  component: DashboardHomeComponent, canActivate: [ManagerGuard] },
    { path: 'users', component: UsersComponent, canActivate: [ManagerGuard] },
    { path: 'users/create', component: CreateEmployeeComponent, canActivate: [ManagerGuard] },
    { path: 'users/:id', component: UserDetailsComponent, canActivate: [ManagerGuard] },
     { path: 'clients', component: ClientsComponent, canActivate: [ManagerGuard] },
    { path: 'clients/:id', component: ClientDetailsComponent, canActivate: [ManagerGuard] },
    { path: 'tickets', component: TicketsComponent, canActivate: [AuthGuard] },
    { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
    { path: 'tickets/:id', component: TicketDetailsComponent, canActivate: [AuthGuard] },
    { path: 'products', component: ProductsComponent, canActivate: [AuthGuard] },
  ] },
  { path: '**', redirectTo: 'user/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
