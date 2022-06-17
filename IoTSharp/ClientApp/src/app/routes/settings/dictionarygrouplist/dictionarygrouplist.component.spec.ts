import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DictionarygrouplistComponent } from './dictionarygrouplist.component';

describe('DictionarygrouplistComponent', () => {
  let component: DictionarygrouplistComponent;
  let fixture: ComponentFixture<DictionarygrouplistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DictionarygrouplistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DictionarygrouplistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
