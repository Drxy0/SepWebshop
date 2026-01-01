import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { User } from '../../services/user/user';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.html',
})
export class Register {
  loading = false;
  generalError: string | null = null;
  fieldErrors: Record<string, string[]> = {};

  form;

  constructor(private fb: FormBuilder, private user: User, private router: Router) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      name: ['', Validators.required],
      surname: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  submit() {
    this.generalError = null;
    this.fieldErrors = {};

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.form.disable();

    this.user.registerUser(this.form.value as any).subscribe({
      next: () => {
        this.loading = false;
        this.form.enable();
        this.form.reset();
        this.router.navigate(['/login']);
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.form.enable();

        if (err.error?.code) {
          this.generalError = err.error.description;
          return;
        }

        if (err.error?.errors) {
          this.fieldErrors = err.error.errors;
        }
      },
    });
  }
}
