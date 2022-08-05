import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConditionbuilderComponent } from './conditionbuilder.component';

describe('ConditionbuilderComponent', () => {
  let component: ConditionbuilderComponent;
  let fixture: ComponentFixture<ConditionbuilderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConditionbuilderComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ConditionbuilderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
