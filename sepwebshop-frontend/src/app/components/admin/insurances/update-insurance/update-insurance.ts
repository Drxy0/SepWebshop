import { Component, OnInit, signal, input, output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { InsurancesService } from '../../../../services/insurances/insurances-servis';
import { IAddInsuranceRequest } from '../../../../models/interfaces/insurance';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-update-insurance',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './update-insurance.html',
})
export class UpdateInsurance implements OnInit {
  insuranceId = input<string | null>(null);
  submitted = output<void>();

  loading = signal(false);
  message = signal<string | null>(null);
  messageType = signal<'success' | 'error' | null>(null);
  fieldErrors = signal<Record<string, string[]>>({});

  private fb = inject(FormBuilder);
  private service = inject(InsurancesService);

  form = this.fb.group({
    name: ['', Validators.required],
    description: ['', Validators.required],
    pricePerDay: [0, Validators.required],
    deductibleAmount: [0, Validators.required],
  });

  ngOnInit() {
    const id = this.insuranceId();
    if (id) {
      this.loading.set(true);
      this.service
        .getInsurance(id)
        .pipe(finalize(() => this.loading.set(false)))
        .subscribe((data) => this.form.patchValue(data));
    }
  }

  submit() {
    const id = this.insuranceId();
    if (!id || this.form.invalid) return;

    this.loading.set(true);
    this.message.set(null);

    this.service
      .updateInsurance(id, this.form.value as IAddInsuranceRequest)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => {
          this.message.set('Updated successfully!');
          this.messageType.set('success');
          this.submitted.emit();
        },
        error: (err: HttpErrorResponse) => {
          this.message.set('Update failed');
          this.messageType.set('error');
          if (err.error?.errors) this.fieldErrors.set(err.error.errors);
        },
      });
  }
}
