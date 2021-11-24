import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchformgeneratorComponent } from './searchformgenerator.component';

describe('SearchformgeneratorComponent', () => {
  let component: SearchformgeneratorComponent;
  let fixture: ComponentFixture<SearchformgeneratorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SearchformgeneratorComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SearchformgeneratorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
