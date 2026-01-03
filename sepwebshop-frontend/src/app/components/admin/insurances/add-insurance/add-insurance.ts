import { Component, output, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { InsurancesService } from '../../../../services/insurances/insurances-servis';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs/operators';
import { IAddInsuranceRequest } from '../../../../models/interfaces/insurance';

@Component({
  selector: 'app-add-insurance',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-insurance.html',
})
export class AddInsurance {
  loading = signal(false);
  message = signal<string | null>(null);
  messageType = signal<'success' | 'error' | null>(null);
  fieldErrors = signal<Record<string, string[]>>({});

  submitted = output<void>();

  private fb = inject(FormBuilder);
  private service = inject(InsurancesService);

  form = this.fb.group({
    name: ['', Validators.required],
    description: ['', Validators.required],
    pricePerDay: [0, [Validators.required, Validators.min(0)]],
    deductibleAmount: [0, [Validators.required, Validators.min(0)]],
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.message.set(null);
    this.fieldErrors.set({});

    const payload: IAddInsuranceRequest = {
      name: this.form.value.name!,
      description: this.form.value.description!,
      pricePerDay: Number(this.form.value.pricePerDay),
      deductibleAmount: Number(this.form.value.deductibleAmount),
    };

    this.service
      .addInsurance(payload)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => {
          this.message.set('Insurance added successfully!');
          this.messageType.set('success');
          this.form.reset({ pricePerDay: 0, deductibleAmount: 0 });
          this.submitted.emit();
        },
        error: (err: HttpErrorResponse) => {
          this.message.set('Error adding insurance');
          this.messageType.set('error');
          if (err.error?.errors) this.fieldErrors.set(err.error.errors);
        },
      });
  }
}
