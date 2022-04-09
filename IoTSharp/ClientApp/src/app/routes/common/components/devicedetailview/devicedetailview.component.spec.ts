import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicedetailviewComponent } from './devicedetailview.component';

describe('DevicedetailviewComponent', () => {
  let component: DevicedetailviewComponent;
  let fixture: ComponentFixture<DevicedetailviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevicedetailviewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicedetailviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
