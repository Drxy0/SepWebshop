import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RentCar } from './rent-car';

describe('RentCar', () => {
  let component: RentCar;
  let fixture: ComponentFixture<RentCar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RentCar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RentCar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
