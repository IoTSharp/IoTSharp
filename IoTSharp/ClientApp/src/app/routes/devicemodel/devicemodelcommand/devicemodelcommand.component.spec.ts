import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicemodelcommandComponent } from './devicemodelcommand.component';

describe('DevicemodelcommandComponent', () => {
  let component: DevicemodelcommandComponent;
  let fixture: ComponentFixture<DevicemodelcommandComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevicemodelcommandComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicemodelcommandComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
