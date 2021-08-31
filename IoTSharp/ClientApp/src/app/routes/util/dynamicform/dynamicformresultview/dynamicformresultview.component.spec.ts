import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicformresultviewComponent } from './dynamicformresultview.component';

describe('DynamicformresultviewComponent', () => {
  let component: DynamicformresultviewComponent;
  let fixture: ComponentFixture<DynamicformresultviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DynamicformresultviewComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamicformresultviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
