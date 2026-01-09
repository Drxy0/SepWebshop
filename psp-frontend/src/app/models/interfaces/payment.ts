export interface IPaymentMethod {
  id: number;
  name: string;
  selected: boolean;
}

export interface IUpdatePaymentRequest {
  paymentMethodIds: number[];
}
