import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicemodelformComponent } from './devicemodelform.component';

describe('DevicemodelformComponent', () => {
  let component: DevicemodelformComponent;
  let fixture: ComponentFixture<DevicemodelformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevicemodelformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicemodelformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
