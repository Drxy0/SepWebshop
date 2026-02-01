import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pay-crypto',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pay-crypto.html',
  styleUrl: './pay-crypto.css',
})
export class PayCrypto {
  orderId = signal<string | null>(null);
  qrImageUrl = signal<string | null>(null);
  isLoading = signal(true);

  constructor() {
    const state = history.state as { qrUrl?: string; orderId?: string };

    if (state?.orderId) {
      this.orderId.set(state.orderId);
    }

    if (state?.qrUrl) {
      this.qrImageUrl.set(state.qrUrl);
      this.isLoading.set(false);
    }
  }
}
