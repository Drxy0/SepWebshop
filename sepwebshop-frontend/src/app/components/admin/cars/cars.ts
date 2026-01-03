import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CarsService } from '../../../services/cars/cars';
import { AddCar } from './add-car/add-car';
import { UpdateCar } from './update-car/update-car';
import { ICarResponse, IAddCarRequest } from '../../../models/interfaces/car';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-cars',
  standalone: true,
  imports: [CommonModule, AddCar, UpdateCar],
  templateUrl: './cars.html',
})
export class CarsComponent {
  cars = signal<ICarResponse[]>([]);
  loading = signal(false); // loader za listu automobila

  showAddCarModal = signal(false);
  showUpdateCarModal = signal(false);
  showDeleteModal = signal(false);
  selectedCarId: string | null = null;

  constructor(private carsService: CarsService) {
    this.loadCars();
  }

  loadCars() {
    this.loading.set(true);
    this.carsService
      .getAllCars()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => this.cars.set(data), // Koristi .set()
        error: () => this.loading.set(false),
      });
  }

  openAddCar() {
    this.showAddCarModal.set(true);
  }

  closeAddCar() {
    this.showAddCarModal.set(false);
  }

  openUpdateCar(id: string) {
    this.selectedCarId = id;
    this.showUpdateCarModal.set(true);
  }

  closeUpdateCar() {
    this.showUpdateCarModal.set(false);
    this.selectedCarId = null;
  }

  openDeleteConfirm(id: string) {
    this.selectedCarId = id;
    this.showDeleteModal.set(true);
  }

  // 2. Zatvara modal bez brisanja
  closeDeleteModal() {
    this.showDeleteModal.set(false);
    this.selectedCarId = null;
  }

  // 3. Prava funkcija koja briše nakon potvrde
  confirmDelete() {
    if (!this.selectedCarId) return;

    this.loading.set(true);
    this.showDeleteModal.set(false); // Zatvori modal odmah

    this.carsService
      .deleteCar(this.selectedCarId)
      .pipe(
        finalize(() => {
          this.loading.set(false);
          this.selectedCarId = null;
        })
      )
      .subscribe({
        next: () => this.loadCars(),
        error: () => alert('Error deleting car'),
      });
  }
}
