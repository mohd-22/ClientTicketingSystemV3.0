import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../shared/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { Router, RouteReuseStrategy } from '@angular/router';

interface GenderOption {
  label: string;
  value: number;
}

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.css']
})
export class RegistrationComponent implements OnInit {
  registrationForm!: FormGroup;
  submitted = false;
  successMessage = '';
  errorMessage = '';
  loading = false;
  private readonly PhonePattern = /^(?:\+9627|07)\d{8}$/;
  genderOptions: GenderOption[] = [
    { label: 'Male', value: 1 },
    { label: 'Female', value: 2 },
  ];

  constructor(private formBuilder: FormBuilder, private authService: AuthService, private toastr: ToastrService, private router: Router) { }

  
  ngOnInit(): void {
    this.registrationForm = this.formBuilder.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      userName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern(this.PhonePattern)]],
      address: ['', [Validators.required, Validators.minLength(3)]],
      dateOfBirth: ['', Validators.required],
      gender: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      imageUrl: ['']
    }, { validators: this.passwordMatchValidator });
  }
  

  passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  get f() {
    return this.registrationForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    this.successMessage = '';
    this.errorMessage = '';

    if (this.registrationForm.invalid) {
      return;
    }

    this.loading = true;

    const payload = {
      fullName: this.registrationForm.value.fullName,
      userName: this.registrationForm.value.userName,
      email: this.registrationForm.value.email,
      phoneNumber: this.registrationForm.value.phoneNumber,
      address: this.registrationForm.value.address,
      dateOfBirth: this.registrationForm.value.dateOfBirth,
      gender: Number(this.registrationForm.value.gender),
      password: this.registrationForm.value.password,
      confirmPassword: this.registrationForm.value.confirmPassword,
      imageUrl: this.registrationForm.value.imageUrl || null,
    };

    this.authService.register(payload).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = response?.message || 'Registration successful!';
        this.registrationForm.reset();
        this.submitted = false;
        this.toastr.success(this.successMessage, 'Success', { timeOut: 3000 });
        this.router.navigate(['/user/login']);
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Registration failed. Please try again.';
        this.toastr.error(this.errorMessage, 'Error', { timeOut: 3000 });
      },
    });
  }
}
