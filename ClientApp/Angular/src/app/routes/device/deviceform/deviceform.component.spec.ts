import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeviceformComponent } from './deviceform.component';

describe('DeviceformComponent', () => {
  let component: DeviceformComponent;
  let fixture: ComponentFixture<DeviceformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DeviceformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DeviceformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
