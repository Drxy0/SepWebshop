import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Insurances } from './insurances';

describe('Insurances', () => {
  let component: Insurances;
  let fixture: ComponentFixture<Insurances>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Insurances]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Insurances);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
