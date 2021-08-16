import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CodefieldComponent } from './codefield.component';

describe('CodefieldComponent', () => {
  let component: CodefieldComponent;
  let fixture: ComponentFixture<CodefieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CodefieldComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CodefieldComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
