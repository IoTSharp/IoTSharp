import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevivceshapeComponent } from './devivceshape.component';

describe('DevivceshapeComponent', () => {
  let component: DevivceshapeComponent;
  let fixture: ComponentFixture<DevivceshapeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevivceshapeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DevivceshapeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
