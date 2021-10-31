import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TasktesterComponent } from './tasktester.component';

describe('TasktesterComponent', () => {
  let component: TasktesterComponent;
  let fixture: ComponentFixture<TasktesterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TasktesterComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TasktesterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
