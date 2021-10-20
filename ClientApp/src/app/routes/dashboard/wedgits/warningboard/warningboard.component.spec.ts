import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WarningboardComponent } from './warningboard.component';

describe('WarningboardComponent', () => {
  let component: WarningboardComponent;
  let fixture: ComponentFixture<WarningboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ WarningboardComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(WarningboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
