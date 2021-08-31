import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WidgetcustomerComponent } from './widgetcustomer.component';

describe('WidgetcustomerComponent', () => {
  let component: WidgetcustomerComponent;
  let fixture: ComponentFixture<WidgetcustomerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [WidgetcustomerComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(WidgetcustomerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
