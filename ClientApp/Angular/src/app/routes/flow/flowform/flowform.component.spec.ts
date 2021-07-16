import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FlowformComponent } from './flowform.component';

describe('FlowformComponent', () => {
  let component: FlowformComponent;
  let fixture: ComponentFixture<FlowformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FlowformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FlowformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
