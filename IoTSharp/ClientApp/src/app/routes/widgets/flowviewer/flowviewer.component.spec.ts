import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FlowviewerComponent } from './flowviewer.component';

describe('FlowviewerComponent', () => {
  let component: FlowviewerComponent;
  let fixture: ComponentFixture<FlowviewerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FlowviewerComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FlowviewerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
