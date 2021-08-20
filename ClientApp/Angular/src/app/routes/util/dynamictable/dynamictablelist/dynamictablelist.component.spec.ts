import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamictablelistComponent } from './dynamictablelist.component';

describe('DynamictablelistComponent', () => {
  let component: DynamictablelistComponent;
  let fixture: ComponentFixture<DynamictablelistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DynamictablelistComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamictablelistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
