import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FieldpartComponent } from './fieldpart.component';

describe('FieldpartComponent', () => {
  let component: FieldpartComponent;
  let fixture: ComponentFixture<FieldpartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [FieldpartComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FieldpartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
