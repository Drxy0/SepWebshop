import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateCar } from './update-car';

describe('UpdateCar', () => {
  let component: UpdateCar;
  let fixture: ComponentFixture<UpdateCar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdateCar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UpdateCar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
