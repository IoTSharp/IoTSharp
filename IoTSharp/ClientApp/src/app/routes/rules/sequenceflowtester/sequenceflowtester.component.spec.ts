import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SequenceflowtesterComponent } from './sequenceflowtester.component';

describe('SequenceflowtesterComponent', () => {
  let component: SequenceflowtesterComponent;
  let fixture: ComponentFixture<SequenceflowtesterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SequenceflowtesterComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SequenceflowtesterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
