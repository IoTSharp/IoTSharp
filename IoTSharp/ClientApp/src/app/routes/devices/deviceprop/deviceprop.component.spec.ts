import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicepropComponent } from './deviceprop.component';

describe('DevicepropComponent', () => {
  let component: DevicepropComponent;
  let fixture: ComponentFixture<DevicepropComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevicepropComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicepropComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
