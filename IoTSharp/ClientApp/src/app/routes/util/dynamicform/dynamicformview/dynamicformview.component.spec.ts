import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicformviewComponent } from './dynamicformview.component';

describe('DynamicformviewComponent', () => {
  let component: DynamicformviewComponent;
  let fixture: ComponentFixture<DynamicformviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DynamicformviewComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamicformviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
