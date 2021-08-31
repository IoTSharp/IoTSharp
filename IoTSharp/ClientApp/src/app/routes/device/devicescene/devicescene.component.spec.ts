import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicesceneComponent } from './devicescene.component';

describe('DevicesceneComponent', () => {
  let component: DevicesceneComponent;
  let fixture: ComponentFixture<DevicesceneComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DevicesceneComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicesceneComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
