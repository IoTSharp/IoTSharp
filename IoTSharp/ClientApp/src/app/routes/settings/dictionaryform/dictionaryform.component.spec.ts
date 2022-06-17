import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DictionaryformComponent } from './dictionaryform.component';

describe('DictionaryformComponent', () => {
  let component: DictionaryformComponent;
  let fixture: ComponentFixture<DictionaryformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DictionaryformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DictionaryformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
