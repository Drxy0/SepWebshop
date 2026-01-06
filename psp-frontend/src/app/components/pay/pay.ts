import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-pay',
  standalone: true,
  imports: [],
  templateUrl: './pay.html',
  styleUrl: './pay.css',
})
export class Pay implements OnInit {
  private route = inject(ActivatedRoute);

  orderId = signal<string | null>(null);
  merchantId = signal<string | null>(null);

  ngOnInit() {
    // Čitamo oba parametra iz URL-a
    const params = this.route.snapshot.queryParamMap;

    this.orderId.set(params.get('orderId'));
    this.merchantId.set(params.get('merchantId'));

    console.log('PSP podaci:', {
      order: this.orderId(),
      merchant: this.merchantId(),
    });
  }
}
