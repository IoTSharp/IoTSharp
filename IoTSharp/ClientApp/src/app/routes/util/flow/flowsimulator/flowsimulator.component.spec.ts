import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FlowsimulatorComponent } from './flowsimulator.component';

describe('FlowsimulatorComponent', () => {
  let component: FlowsimulatorComponent;
  let fixture: ComponentFixture<FlowsimulatorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [FlowsimulatorComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FlowsimulatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
