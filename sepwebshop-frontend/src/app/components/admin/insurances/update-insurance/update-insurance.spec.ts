import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateInsurance } from './update-insurance';

describe('UpdateInsurance', () => {
  let component: UpdateInsurance;
  let fixture: ComponentFixture<UpdateInsurance>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdateInsurance]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UpdateInsurance);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
