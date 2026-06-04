import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../shared/services/auth.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  submitted = false;
  loading = false;
  errorMessage = '';
  showPassword = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loginForm = this.formBuilder.group({
      emailOrUsername: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  get f() {
    return this.loginForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';

    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;

    this.authService.login({
      emailOrUsername: this.loginForm.value.emailOrUsername.trim(),
      password: this.loginForm.value.password
    }).subscribe({
      next: (token: string) => {
        this.loading = false;
        const accessToken = (token ?? '').trim();
        if (accessToken) {
          localStorage.setItem('access_token', accessToken);
        }

        // this.toastr.success('Login successful', 'Success', { timeOut: 3000 });
        void this.router.navigateByUrl('/dashboard', { replaceUrl: true });
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        if (err.status === 404) {
          this.errorMessage = 'User not found.';
        } else if (err.status === 401) {
          this.errorMessage = 'Invalid Email or Password.';
        } else if (err.status === 400) {
          this.errorMessage = 'Your account is deactivated. Please contact support.';
        } else {
          this.errorMessage = 'Internal server error. Please try again later.';
        }
        // this.toastr.error(this.errorMessage, 'Error', { timeOut: 3000 });
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

}
