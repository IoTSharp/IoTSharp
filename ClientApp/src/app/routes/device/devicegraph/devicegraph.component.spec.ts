import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicegraphComponent } from './devicegraph.component';

describe('DevicegraphComponent', () => {
  let component: DevicegraphComponent;
  let fixture: ComponentFixture<DevicegraphComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevicegraphComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicegraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
