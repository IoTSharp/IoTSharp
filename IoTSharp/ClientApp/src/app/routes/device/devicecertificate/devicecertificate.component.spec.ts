import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicecertificateComponent } from './devicecertificate.component';

describe('DevicecertificateComponent', () => {
  let component: DevicecertificateComponent;
  let fixture: ComponentFixture<DevicecertificateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevicecertificateComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicecertificateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
