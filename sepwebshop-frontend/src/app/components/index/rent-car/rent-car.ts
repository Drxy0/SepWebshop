import { Component, Input, OnInit, signal, inject, output, computed } from '@angular/core';
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
import { OrdersService } from '../../../services/orders/orders';
import { IOrderResponse } from '../../../models/interfaces/order';

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
  private ordersService = inject(OrdersService);

  insurances = signal<IInsuranceResponse[]>([]);
  bookedDates = signal<IOrderResponse[]>([]);
  minDate: string = '';

  // Signal koji čuva konkretne termine koji su u konfliktu sa trenutnim unosom
  conflictingOrders = signal<IOrderResponse[]>([]);

  rentForm = this.fb.group(
    {
      insuranceId: ['', Validators.required],
      leaseStartDate: ['', [Validators.required, this.pastDateValidator]],
      leaseEndDate: ['', Validators.required],
    },
    { validators: [this.dateOrderValidator, this.overlapValidator.bind(this)] }
  );

  ngOnInit() {
    this.loadInsurances();
    this.loadBookedDates();

    const today = new Date();
    this.minDate = today.toISOString().split('T')[0];
    this.rentForm.patchValue({ leaseStartDate: this.minDate });
  }

  loadInsurances() {
    this.insuranceService.getAllInsurances().subscribe((data) => {
      this.insurances.set(data);
    });
  }

  loadBookedDates() {
    if (this.selectedCar?.id) {
      this.ordersService.getOrdersByCarId(this.selectedCar.id).subscribe((orders) => {
        this.bookedDates.set(orders);
      });
    }
  }

  overlapValidator(group: AbstractControl): ValidationErrors | null {
    const start = group.get('leaseStartDate')?.value;
    const end = group.get('leaseEndDate')?.value;

    if (!start || !end || this.bookedDates().length === 0) {
      this.conflictingOrders.set([]);
      return null;
    }

    const newStart = new Date(start).getTime();
    const newEnd = new Date(end).getTime();

    const conflicts = this.bookedDates().filter((order) => {
      const existingStart = new Date(order.leaseStartDate).getTime();
      const existingEnd = new Date(order.leaseEndDate).getTime();
      return newStart <= existingEnd && newEnd >= existingStart;
    });

    this.conflictingOrders.set(conflicts);

    return conflicts.length > 0 ? { dateOverlap: true } : null;
  }

  pastDateValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const selected = new Date(control.value);
    return selected < today ? { pastDate: true } : null;
  }

  dateOrderValidator(group: AbstractControl): ValidationErrors | null {
    const start = group.get('leaseStartDate')?.value;
    const end = group.get('leaseEndDate')?.value;
    if (start && end && new Date(start) >= new Date(end)) {
      return { dateOrderInvalid: true };
    }
    return null;
  }

  calculateDays(): number {
    const start = this.rentForm.value.leaseStartDate;
    const end = this.rentForm.value.leaseEndDate;
    if (!start || !end || this.rentForm.invalid) return 0;
    const diffTime = new Date(end).getTime() - new Date(start).getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays > 0 ? diffDays : 0;
  }

  calculateTotal(): number {
    const days = this.calculateDays();
    if (!this.selectedCar || days <= 0) return 0;
    const selectedIns = this.insurances().find((i) => i.id === this.rentForm.value.insuranceId);
    return (
      (this.selectedCar.price + (selectedIns?.pricePerDay || 0)) * days +
      (selectedIns?.deductibleAmount || 0)
    );
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
      currency: 'USD',
    };

    this.ordersService.createOrder(payload).subscribe({
      next: (response) => {
        // Proveravamo da li je backend vratio URL
        if (response && response.paymentUrl) {
          console.log('Redirecting to PSP:', response.paymentUrl);

          // Fizička redirekcija na drugu aplikaciju (PSP)
          window.location.href = response.paymentUrl;
        } else {
          alert('Order created, but no redirect URL received.');
        }
      },
      error: (err) => {
        console.error('Order error:', err);
        alert('Conflict detected on server or payment service is down!');
      },
    });
  }

  cancel() {
    this.onCancel.emit();
  }
}
