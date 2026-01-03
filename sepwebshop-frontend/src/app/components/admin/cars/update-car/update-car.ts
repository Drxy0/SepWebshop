import { Component, OnInit, signal, input, output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { CarsService } from '../../../../services/cars/cars';
import { IAddCarRequest } from '../../../../models/interfaces/car';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-update-car',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './update-car.html',
})
export class UpdateCar implements OnInit {
  // Signali za ulaz i izlaz (Angular 17.1+)
  carId = input<string | null>(null);
  submitted = output<void>();

  // Signali za stanje
  loading = signal(false);
  message = signal<string | null>(null);
  messageType = signal<'success' | 'error' | null>(null);
  fieldErrors = signal<Record<string, string[]>>({});

  private fb = inject(FormBuilder);
  private carsService = inject(CarsService);

  form = this.fb.group({
    brandAndModel: ['', Validators.required],
    year: [new Date().getFullYear(), Validators.required],
    plateNumber: ['', Validators.required],
    price: [0, Validators.required],
  });

  ngOnInit(): void {
    const id = this.carId(); // Čitamo vrednost iz input signala
    if (id) {
      this.loading.set(true);
      this.carsService
        .getCar(id)
        .pipe(finalize(() => this.loading.set(false)))
        .subscribe((car) => {
          this.form.patchValue({
            brandAndModel: car.brandAndModel ?? '',
            year: car.year ?? new Date().getFullYear(),
            plateNumber: car.plateNumber ?? '',
            price: car.price ?? 0,
          });
        });
    }
  }

  submit() {
    const id = this.carId();
    if (!id) return;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.form.disable();
    this.message.set(null);
    this.fieldErrors.set({});

    const value: IAddCarRequest = {
      brandAndModel: this.form.value.brandAndModel ?? '',
      year: Number(this.form.value.year) || new Date().getFullYear(),
      plateNumber: this.form.value.plateNumber ?? '',
      price: Number(this.form.value.price) || 0,
    };

    this.carsService
      .updateCar(id, value)
      .pipe(
        finalize(() => {
          this.loading.set(false);
          this.form.enable();
        })
      )
      .subscribe({
        next: () => {
          this.message.set('Car updated successfully');
          this.messageType.set('success');
          this.submitted.emit(); // Obaveštavamo roditelja da osveži listu
        },
        error: (err: HttpErrorResponse) => {
          this.message.set('Error updating car');
          this.messageType.set('error');
          if (err.error?.errors) this.fieldErrors.set(err.error.errors);
        },
      });
  }
}
