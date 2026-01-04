import { Component, Input, OnInit, signal, inject, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { ICarResponse } from '../../../models/interfaces/car';
import { IInsuranceResponse } from '../../../models/interfaces/insurance';
import { InsurancesService } from '../../../services/insurances/insurances-servis';

@Component({
  selector: 'app-rent-car',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './rent-car.html',
})
export class RentCar implements OnInit {
  @Input() selectedCar: ICarResponse | null = null;
  onCancel = output<void>();

  private fb = inject(FormBuilder);
  private insuranceService = inject(InsurancesService);

  insurances = signal<IInsuranceResponse[]>([]);
  minDate: string = ''; // Za HTML 'min' atribut

  rentForm = this.fb.group(
    {
      insuranceId: ['', Validators.required],
      leaseStartDate: ['', [Validators.required, this.pastDateValidator]], // Dodat validator za prošlost
      leaseEndDate: ['', Validators.required],
    },
    { validators: this.dateOrderValidator }
  );

  ngOnInit() {
    this.loadInsurances();

    // Postavljanje minimalnog datuma na danas
    const today = new Date();
    this.minDate = today.toISOString().split('T')[0];

    // Default start date je danas
    this.rentForm.patchValue({ leaseStartDate: this.minDate });
  }

  // Validator: Proverava da li je izabrani datum pre današnjeg
  pastDateValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;

    const today = new Date();
    today.setHours(0, 0, 0, 0); // Resetujemo vreme na početak dana za poređenje
    const selected = new Date(control.value);

    return selected < today ? { pastDate: true } : null;
  }

  // Validator: Krajnji datum mora biti nakon početnog
  dateOrderValidator(group: AbstractControl): ValidationErrors | null {
    const start = group.get('leaseStartDate')?.value;
    const end = group.get('leaseEndDate')?.value;

    if (start && end && new Date(start) >= new Date(end)) {
      return { dateOrderInvalid: true };
    }
    return null;
  }

  loadInsurances() {
    this.insuranceService.getAllInsurances().subscribe((data) => {
      this.insurances.set(data);
    });
  }

  calculateDays(): number {
    const start = this.rentForm.value.leaseStartDate;
    const end = this.rentForm.value.leaseEndDate;
    if (!start || !end || this.rentForm.errors?.['dateOrderInvalid']) return 0;

    const startDate = new Date(start);
    const endDate = new Date(end);
    const diffTime = endDate.getTime() - startDate.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    return diffDays > 0 ? diffDays : 0;
  }

  calculateTotal(): number {
    const days = this.calculateDays();
    if (!this.selectedCar || days <= 0) return 0;

    const insId = this.rentForm.value.insuranceId;
    const selectedIns = this.insurances().find((i) => i.id === insId);

    const dailyBase = this.selectedCar.price;
    const dailyIns = selectedIns?.pricePerDay || 0;

    return (dailyBase + dailyIns) * days;
  }

  confirmBooking() {
    if (this.rentForm.invalid) {
      this.rentForm.markAllAsTouched();
      return;
    }

    const payload = {
      carId: this.selectedCar?.id,
      insuranceId: this.rentForm.value.insuranceId,
      leaseStartDate: new Date(this.rentForm.value.leaseStartDate!).toISOString(),
      leaseEndDate: new Date(this.rentForm.value.leaseEndDate!).toISOString(),
    };

    console.log('Booking Payload:', payload);
    alert('Booking successful!');
    this.onCancel.emit();
  }

  cancel() {
    this.onCancel.emit();
  }
}
