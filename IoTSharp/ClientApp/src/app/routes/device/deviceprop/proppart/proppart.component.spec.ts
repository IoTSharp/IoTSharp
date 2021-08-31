import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProppartComponent } from './proppart.component';

describe('ProppartComponent', () => {
  let component: ProppartComponent;
  let fixture: ComponentFixture<ProppartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProppartComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProppartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
