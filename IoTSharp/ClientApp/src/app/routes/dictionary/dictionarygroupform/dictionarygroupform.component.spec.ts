import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DictionarygroupformComponent } from './dictionarygroupform.component';

describe('DictionarygroupformComponent', () => {
  let component: DictionarygroupformComponent;
  let fixture: ComponentFixture<DictionarygroupformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DictionarygroupformComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DictionarygroupformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
