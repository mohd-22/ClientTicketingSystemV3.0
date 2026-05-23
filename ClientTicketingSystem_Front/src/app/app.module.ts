import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ToastrModule } from 'ngx-toastr';
import { NgChartsModule } from 'ng2-charts';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { UserComponent } from './user/user.component';
import { RegistrationComponent } from './user/registration/registration.component';
import { LoginComponent } from './user/login/login.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { TicketsComponent } from './dashboard/tickets/tickets.component';
import { DashboardHomeComponent } from './dashboard/dashboard-home/dashboard-home.component';
import { ProductsComponent } from './dashboard/products/products.component';
import { AuthInterceptor } from './shared/interceptors/auth.interceptor';
import { UnauthorizedInterceptor } from './shared/interceptors/unauthorized.interceptor';
import { UsersComponent } from './dashboard/users/users.component';
import { CreateEmployeeComponent } from './dashboard/users/create-employee/create-employee.component';
import { UserDetailsComponent } from './dashboard/users/user-details/user-details.component';
import { ClientsComponent } from './dashboard/clients/clients.component';
import { TicketDetailsComponent } from './dashboard/tickets/ticket-details/ticket-details.component';
import { ClientDetailsComponent } from './dashboard/clients/client-details/client-details.component';
import { ProfileComponent } from './dashboard/profile/profile.component';

@NgModule({
  declarations: [
    AppComponent,
    UserComponent,
    RegistrationComponent,
    LoginComponent,
    DashboardComponent,
    TicketsComponent,
    DashboardHomeComponent,
    ProductsComponent,
    UsersComponent,
    CreateEmployeeComponent,
    UserDetailsComponent,
    ClientsComponent,
    TicketDetailsComponent,
    ClientDetailsComponent
    ,ProfileComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    NgChartsModule,
    AppRoutingModule,
    ReactiveFormsModule,
    FormsModule,
    ToastrModule.forRoot({
    positionClass: 'toast-bottom-right', 
    preventDuplicates: true,            
    timeOut: 3000                       
  })
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: UnauthorizedInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
