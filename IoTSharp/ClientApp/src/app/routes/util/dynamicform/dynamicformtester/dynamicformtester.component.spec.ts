import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicformtesterComponent } from './dynamicformtester.component';

describe('DynamicformtesterComponent', () => {
  let component: DynamicformtesterComponent;
  let fixture: ComponentFixture<DynamicformtesterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DynamicformtesterComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamicformtesterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
