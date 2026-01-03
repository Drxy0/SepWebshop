import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddInsurance } from './add-insurance';

describe('AddInsurance', () => {
  let component: AddInsurance;
  let fixture: ComponentFixture<AddInsurance>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddInsurance]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddInsurance);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
