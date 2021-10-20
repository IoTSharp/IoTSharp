import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicformdesignerComponent } from './dynamicformdesigner.component';

describe('DynamicformdesignerComponent', () => {
  let component: DynamicformdesignerComponent;
  let fixture: ComponentFixture<DynamicformdesignerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DynamicformdesignerComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamicformdesignerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
