import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicemodellistComponent } from './devicemodellist.component';

describe('DevicemodellistComponent', () => {
  let component: DevicemodellistComponent;
  let fixture: ComponentFixture<DevicemodellistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevicemodellistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicemodellistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
