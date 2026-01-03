export interface IAddCarRequest {
  brandAndModel: string;
  year: number;
  plateNumber: string;
  price: number;
}
export interface ICarResponse extends IAddCarRequest {
  id: string;
}
