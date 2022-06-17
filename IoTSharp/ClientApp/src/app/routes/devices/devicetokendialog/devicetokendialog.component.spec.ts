import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicetokendialogComponent } from './devicetokendialog.component';

describe('DevicetokendialogComponent', () => {
  let component: DevicetokendialogComponent;
  let fixture: ComponentFixture<DevicetokendialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevicetokendialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicetokendialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
