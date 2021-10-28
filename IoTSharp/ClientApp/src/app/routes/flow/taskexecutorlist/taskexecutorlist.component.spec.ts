import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskexecutorlistComponent } from './taskexecutorlist.component';

describe('TaskexecutorlistComponent', () => {
  let component: TaskexecutorlistComponent;
  let fixture: ComponentFixture<TaskexecutorlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskexecutorlistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskexecutorlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
