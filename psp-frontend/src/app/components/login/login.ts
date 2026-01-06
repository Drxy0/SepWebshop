import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { User } from '../../services/user/user';
import { Constants } from '../../constants/constants';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
})
export class Login {
  loading = false;
  generalError: string | null = null;

  form;

  constructor(private fb: FormBuilder, private user: User, private router: Router) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  submit() {
    this.generalError = null;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.form.disable();

    this.user.loginUser(this.form.value as any).subscribe({
      next: (res) => {
        this.loading = false;
        this.form.enable();

        // save tokens
        localStorage.setItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN, res.accessToken);

        this.router.navigate(['/index']);
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.form.enable();

        this.generalError = err.error?.description || 'Invalid email or password1';
      },
    });
  }
}
