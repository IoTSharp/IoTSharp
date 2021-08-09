import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicformlistComponent } from './dynamicformlist.component';

describe('DynamicformlistComponent', () => {
  let component: DynamicformlistComponent;
  let fixture: ComponentFixture<DynamicformlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DynamicformlistComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamicformlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
