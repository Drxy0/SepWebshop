export interface IInsuranceResponse {
  id: string;
  name: string;
  description: string;
  pricePerDay: number;
  deductibleAmount: number;
}

export interface IAddInsuranceRequest {
  name: string;
  description: string;
  pricePerDay: number;
  deductibleAmount: number;
}
