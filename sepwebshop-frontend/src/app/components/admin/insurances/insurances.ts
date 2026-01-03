import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InsurancesService } from '../../../services/insurances/insurances-servis';
import { AddInsurance } from './add-insurance/add-insurance';
import { UpdateInsurance } from './update-insurance/update-insurance';
import { IInsuranceResponse } from '../../../models/interfaces/insurance';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-insurances',
  standalone: true,
  imports: [CommonModule, AddInsurance, UpdateInsurance],
  templateUrl: './insurances.html',
})
export class Insurances {
  insurances = signal<IInsuranceResponse[]>([]);
  loading = signal(false);

  showAddModal = signal(false);
  showUpdateModal = signal(false);
  showDeleteModal = signal(false);
  selectedId: string | null = null;

  private insurancesService = inject(InsurancesService);

  constructor() {
    this.loadInsurances();
  }

  loadInsurances() {
    this.loading.set(true);
    this.insurancesService
      .getAllInsurances()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => this.insurances.set(data),
        error: () => this.loading.set(false),
      });
  }

  openAdd() {
    this.showAddModal.set(true);
  }
  closeAdd() {
    this.showAddModal.set(false);
  }

  openUpdate(id: string) {
    this.selectedId = id;
    this.showUpdateModal.set(true);
  }
  closeUpdate() {
    this.showUpdateModal.set(false);
    this.selectedId = null;
  }

  openDelete(id: string) {
    this.selectedId = id;
    this.showDeleteModal.set(true);
  }
  closeDelete() {
    this.showDeleteModal.set(false);
    this.selectedId = null;
  }

  confirmDelete() {
    if (!this.selectedId) return;
    this.loading.set(true);
    this.showDeleteModal.set(false);

    this.insurancesService
      .deleteInsurance(this.selectedId)
      .pipe(
        finalize(() => {
          this.loading.set(false);
          this.selectedId = null;
        })
      )
      .subscribe({
        next: () => this.loadInsurances(),
        error: () => alert('Error deleting insurance'),
      });
  }
}
