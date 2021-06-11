import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomerlistComponent } from './customerlist.component';

describe('CustomerlistComponent', () => {
  let component: CustomerlistComponent;
  let fixture: ComponentFixture<CustomerlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CustomerlistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CustomerlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
