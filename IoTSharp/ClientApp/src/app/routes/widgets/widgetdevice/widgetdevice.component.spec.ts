import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WidgetdeviceComponent } from './widgetdevice.component';

describe('WidgetdeviceComponent', () => {
  let component: WidgetdeviceComponent;
  let fixture: ComponentFixture<WidgetdeviceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ WidgetdeviceComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(WidgetdeviceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
