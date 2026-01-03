import { TestBed } from '@angular/core/testing';

import { InsurancesServis } from './insurances-servis';

describe('InsurancesServis', () => {
  let service: InsurancesServis;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(InsurancesServis);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
