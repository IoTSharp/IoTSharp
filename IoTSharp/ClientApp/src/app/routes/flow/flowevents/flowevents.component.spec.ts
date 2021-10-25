import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FloweventsComponent } from './flowevents.component';

describe('FloweventsComponent', () => {
  let component: FloweventsComponent;
  let fixture: ComponentFixture<FloweventsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FloweventsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FloweventsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
