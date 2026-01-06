export interface IOrderResponse {
  id: string;
  userId: string;
  carId: string;
  insuranceId: string;
  leaseStartDate: string;
  leaseEndDate: string;
  totalPrice: number;
  isCompleted: boolean;
  paymentMethod: number;
}

export interface IOrderCreateResponse {
  paymentUrl: string;
}
