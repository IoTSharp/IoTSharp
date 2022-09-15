import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicecertdialogComponent } from './devicecertdialog.component';

describe('DevicecertdialogComponent', () => {
  let component: DevicecertdialogComponent;
  let fixture: ComponentFixture<DevicecertdialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevicecertdialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicecertdialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
