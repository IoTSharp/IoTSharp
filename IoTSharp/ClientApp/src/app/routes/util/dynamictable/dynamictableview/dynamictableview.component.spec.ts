import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamictableviewComponent } from './dynamictableview.component';

describe('DynamictableviewComponent', () => {
  let component: DynamictableviewComponent;
  let fixture: ComponentFixture<DynamictableviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DynamictableviewComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamictableviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
