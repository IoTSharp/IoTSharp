import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ScriptlistComponent } from './scriptlist.component';

describe('ScriptlistComponent', () => {
  let component: ScriptlistComponent;
  let fixture: ComponentFixture<ScriptlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ScriptlistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ScriptlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
