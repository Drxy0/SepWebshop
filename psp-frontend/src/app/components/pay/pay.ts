import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { User } from '../../services/user/user';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pay',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pay.html',
})
export class Pay implements OnInit {
  private route = inject(ActivatedRoute);
  private userService = inject(User);

  orderId = signal<string | null>(null);
  merchantId = signal<string | null>(null);
  availableMethods = signal<string[]>([]);

  ngOnInit() {
    const params = this.route.snapshot.queryParamMap;
    this.orderId.set(params.get('orderId'));
    const mId = params.get('merchantId');
    this.merchantId.set(mId);

    if (mId) {
      this.loadMerchantMethods(mId);
    }
  }

  loadMerchantMethods(id: string) {
    this.userService.getActiveMethods(id).subscribe({
      next: (methods) => {
        this.availableMethods.set(methods);
      },
      error: (err) => {
        console.error('Error fetching merchant methods:', err);
      },
    });
  }

  onPay(method: string) {
    console.log(`Starting payment for ${this.orderId()} using ${method}`);
    // Ovde bi išla logika za procesuiranje uplate
  }
}
