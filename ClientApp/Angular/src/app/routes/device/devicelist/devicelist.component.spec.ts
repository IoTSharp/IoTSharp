import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicelistComponent } from './devicelist.component';

describe('DevicelistComponent', () => {
  let component: DevicelistComponent;
  let fixture: ComponentFixture<DevicelistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevicelistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicelistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
