import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicformeditorComponent } from './dynamicformeditor.component';

describe('DynamicformeditorComponent', () => {
  let component: DynamicformeditorComponent;
  let fixture: ComponentFixture<DynamicformeditorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DynamicformeditorComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamicformeditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
