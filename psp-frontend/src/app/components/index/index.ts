import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { User } from '../../services/user/user';
import { IPaymentMethod } from '../../models/interfaces/payment';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Constants } from '../../constants/constants';

@Component({
  selector: 'app-index',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './index.html',
})
export class Index implements OnInit {
  methods = signal<IPaymentMethod[]>([]);
  loading = signal<boolean>(false);
  message = signal<string>('');
  errorMessage = signal<string>(''); // Novi signal za greške

  constructor(private userService: User, private router: Router) {}

  ngOnInit() {
    this.loadMethods();
  }

  loadMethods() {
    this.loading.set(true);
    this.userService.getPaymentMethods().subscribe({
      next: (res) => {
        this.methods.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load payment methods.');
      },
    });
  }

  saveChanges() {
    this.message.set('');
    this.errorMessage.set('');

    const selectedIds = this.methods()
      .filter((m) => m.selected)
      .map((m) => m.id);

    if (selectedIds.length === 0) {
      this.errorMessage.set('At least one payment method must be selected.');
      return;
    }

    this.loading.set(true);
    this.userService.updatePaymentMethods(selectedIds).subscribe({
      next: () => {
        this.loading.set(false);
        this.message.set('Payment methods updated successfully!');
        setTimeout(() => this.message.set(''), 3000);
      },
      error: (err) => {
        this.loading.set(false);
        const errorMsg = err.error?.message || 'An error occurred while saving changes.';
        this.errorMessage.set(errorMsg);

        setTimeout(() => this.errorMessage.set(''), 5000);
      },
    });
  }

  logout() {
    localStorage.removeItem(Constants.LOCAL_STORAGE_ACCESS_TOKEN);
    this.router.navigate(['/login']);
  }
}
