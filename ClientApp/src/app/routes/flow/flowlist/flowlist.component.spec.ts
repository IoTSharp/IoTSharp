import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FlowlistComponent } from './flowlist.component';

describe('FlowlistComponent', () => {
  let component: FlowlistComponent;
  let fixture: ComponentFixture<FlowlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [FlowlistComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FlowlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
