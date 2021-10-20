import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamictablecolumeditorComponent } from './dynamictablecolumeditor.component';

describe('DynamictablecolumeditorComponent', () => {
  let component: DynamictablecolumeditorComponent;
  let fixture: ComponentFixture<DynamictablecolumeditorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DynamictablecolumeditorComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamictablecolumeditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
