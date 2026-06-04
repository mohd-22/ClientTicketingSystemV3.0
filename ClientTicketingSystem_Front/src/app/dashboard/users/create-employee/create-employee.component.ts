import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ageRangeValidator } from '../../../shared/validators/age.validator';
import { UsersService } from '../../../shared/services/users.service';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

interface GenderOption {
  label: string;
  value: number;
}

@Component({
  selector: 'app-create-employee',
  templateUrl: './create-employee.component.html',
  styles: [
  ]
})
export class CreateEmployeeComponent implements OnInit {

  createForm!: FormGroup;
  submitted = false;
  loading = false;
  errorMessage = '';
  genderOptions: GenderOption[] = [
    { label: 'Male', value: 1 },
    { label: 'Female', value: 2 }
  ];
private readonly PhonePattern = /^07(8|9|7)\d{7}$/;
  constructor(
    private fb: FormBuilder,
    private usersService: UsersService,
    private toastr: ToastrService,
    public router: Router
  ) { }

  ngOnInit(): void {
    this.createForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      userName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern(this.PhonePattern)]],
      address: ['', [Validators.required, Validators.minLength(3)]],
      dateOfBirth: ['', [Validators.required, ageRangeValidator(18, 90)]],
      gender: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      
    }, { validators: this.passwordMatchValidator });
  }



  passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  get f(): any { return this.createForm.controls; }

  onFileSelected(event: Event): void {
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      this.toastr.error('Please complete all required fields correctly.', 'Validation');
      return;
    }

    this.loading = true;
    const payload = {
      fullName: this.createForm.value.fullName,
      userName: this.createForm.value.userName,
      email: this.createForm.value.email,
      phoneNumber: this.createForm.value.phoneNumber,
      address: this.createForm.value.address,
      dateOfBirth: this.createForm.value.dateOfBirth,
      gender: Number(this.createForm.value.gender),
      password: this.createForm.value.password,
      confirmPassword: this.createForm.value.confirmPassword,
      
    };

    console.log('CreateEmployee payload:', payload);
    this.usersService.createEmployee(payload).subscribe({
      next: (response) => {
        this.loading = false;
        console.log('CreateEmployee response:', response);
        if (response && response.success) {
          this.toastr.success(response.message || 'Employee created', 'Success');
          this.createForm.reset();
          this.submitted = false;
          void this.router.navigate(['/dashboard/users']);
        } else {
          this.errorMessage = response?.message || 'Failed to create employee.';
          if (response?.errors && response.errors.length) {
            this.errorMessage = response.errors.join('; ');
          }
          this.toastr.error(this.errorMessage, 'Error');
        }
      },
      error: (err) => {
        this.loading = false;
        console.error('CreateEmployee error:', err);
        this.errorMessage = err?.error?.message || 'Failed to create employee.';
        this.toastr.error(this.errorMessage, 'Error');
      }
    });
  }

}
