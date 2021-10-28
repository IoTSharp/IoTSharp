import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskexecutorformComponent } from './taskexecutorform.component';

describe('TaskexecutorformComponent', () => {
  let component: TaskexecutorformComponent;
  let fixture: ComponentFixture<TaskexecutorformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskexecutorformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskexecutorformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
