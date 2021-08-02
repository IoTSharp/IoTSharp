import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DictionarylistComponent } from './dictionarylist.component';

describe('DictionarylistComponent', () => {
  let component: DictionarylistComponent;
  let fixture: ComponentFixture<DictionarylistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DictionarylistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DictionarylistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
