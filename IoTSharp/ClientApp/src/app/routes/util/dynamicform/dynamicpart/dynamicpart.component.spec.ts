import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicpartComponent } from './dynamicpart.component';

describe('DynamicpartComponent', () => {
  let component: DynamicpartComponent;
  let fixture: ComponentFixture<DynamicpartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DynamicpartComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamicpartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
