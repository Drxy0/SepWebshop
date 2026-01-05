import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CarsService } from '../../services/cars/cars';
import { ICarResponse } from '../../models/interfaces/car';
import { finalize } from 'rxjs';
import { RentCar } from './rent-car/rent-car';

@Component({
  selector: 'app-index',
  standalone: true,
  imports: [CommonModule, RentCar],
  templateUrl: './index.html',
})
export class Index {
  private carsService = inject(CarsService);

  cars = signal<ICarResponse[]>([]);
  loading = signal(false);
  showRentModal = signal(false);
  selectedCar: ICarResponse | null = null;

  constructor() {
    this.loadCars();
  }

  loadCars() {
    this.loading.set(true);
    this.carsService
      .getAllCars()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe((data) => this.cars.set(data));
  }

  openRentModal(car: ICarResponse) {
    this.selectedCar = car;
    this.showRentModal.set(true);
  }

  closeRentModal() {
    this.showRentModal.set(false);
    this.selectedCar = null;
  }
}
