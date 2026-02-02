import { Component, OnInit, inject, signal } from '@angular/core'; // Dodat signal
import { CommonModule } from '@angular/common';
import { OrdersService } from '../../services/orders/orders';
import { CarsService } from '../../services/cars/cars';
import { InsurancesService } from '../../services/insurances/insurances-servis';
import { Auth } from '../../services/auth/auth';
import { forkJoin, map, switchMap, of, finalize } from 'rxjs';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-orders.html',
  styleUrl: './my-orders.css',
})
export class MyOrders implements OnInit {
  private ordersService = inject(OrdersService);
  private carsService = inject(CarsService);
  private insurancesService = inject(InsurancesService);
  private auth = inject(Auth);

  // Signali za stanje komponente
  ordersDetails = signal<any[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit() {
    const userId = this.auth.getUserId();
    if (!userId) {
      this.error.set('User not found.');
      this.loading.set(false);
      return;
    }

    this.ordersService
      .getOrdersByUserId(userId)
      .pipe(
        switchMap((orders) => {
          if (orders.length === 0) return of([]);

          const detailRequests = orders.map((order) =>
            forkJoin({
              car: this.carsService.getCar(order.carId),
              insurance: this.insurancesService.getInsurance(order.insuranceId),
            }).pipe(
              map((res) => ({
                ...order,
                carName: res.car.brandAndModel,
                insuranceName: res.insurance.name,
              })),
            ),
          );
          return forkJoin(detailRequests);
        }),
        // finalize se izvršava bez obzira na uspeh ili grešku
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => {
          // Sortiramo odmah pri postavljanju signala
          const sortedData = data.sort(
            (a: any, b: any) =>
              new Date(b.leaseStartDate).getTime() - new Date(a.leaseStartDate).getTime(),
          );
          this.ordersDetails.set(sortedData);
        },
        error: (err) => {
          console.error('Failed to load orders:', err);
          this.error.set('Failed to load orders. Please try again later.');
        },
      });
  }

  getStatusClasses(status: string): string {
    switch (status) {
      case 'Completed':
        return 'bg-green-200 text-green-800';
      case 'Cancelled':
        return 'bg-red-200 text-red-800';
      case 'Failed':
        return 'bg-yellow-200 text-yellow-800';
      case 'PendingPayment':
        return 'bg-blue-200 text-blue-800';
      case 'Processing':
        return 'bg-indigo-200 text-indigo-800';
      default:
        return 'bg-gray-200 text-gray-800';
    }
  }
}
