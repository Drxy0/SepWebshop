import { Component, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { CarsService } from '../../../../services/cars/cars';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs/operators';
import { IAddCarRequest } from '../../../../models/interfaces/car';

@Component({
  selector: 'app-add-car',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-car.html',
})
export class AddCar {
  // Signali za stanje komponente
  loading = signal(false);
  message = signal<string | null>(null);
  messageType = signal<'success' | 'error' | null>(null);
  fieldErrors = signal<Record<string, string[]>>({});

  // Output događaj (moderni Signal-based output)
  submitted = output<void>();

  form;

  constructor(private fb: FormBuilder, private carsService: CarsService) {
    this.form = this.fb.group({
      brandAndModel: ['', Validators.required],
      year: [new Date().getFullYear(), Validators.required],
      plateNumber: ['', Validators.required],
      price: [0, Validators.required],
    });
  }

  submit() {
    // Resetovanje stanja pre slanja
    this.message.set(null);
    this.fieldErrors.set({});

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.message.set('Please fix the errors below');
      this.messageType.set('error');
      return;
    }

    this.loading.set(true);
    this.form.disable();

    const payload: IAddCarRequest = {
      brandAndModel: this.form.value.brandAndModel || '',
      year: Number(this.form.value.year) || new Date().getFullYear(),
      plateNumber: this.form.value.plateNumber || '',
      price: Number(this.form.value.price) || 0,
    };

    this.carsService
      .addCar(payload)
      .pipe(
        finalize(() => {
          this.loading.set(false);
          this.form.enable();
        })
      )
      .subscribe({
        next: () => {
          this.message.set('Car added successfully');
          this.messageType.set('success');
          this.form.reset({ year: new Date().getFullYear(), price: 0 });

          // Ključni momenat: Obaveštavamo roditelja da osveži listu
          this.submitted.emit();
        },
        error: (err: HttpErrorResponse) => {
          this.message.set('Error while adding car');
          this.messageType.set('error');
          if (err.error?.errors) {
            this.fieldErrors.set(err.error.errors);
          }
        },
      });
  }
}
