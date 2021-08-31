import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamictabletesterComponent } from './dynamictabletester.component';

describe('DynamictabletesterComponent', () => {
  let component: DynamictabletesterComponent;
  let fixture: ComponentFixture<DynamictabletesterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DynamictabletesterComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamictabletesterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
