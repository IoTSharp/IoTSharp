import { ComponentFixture, TestBed } from '@angular/core/testing';

import { I18nlistComponent } from './i18nlist.component';

describe('I18nlistComponent', () => {
  let component: I18nlistComponent;
  let fixture: ComponentFixture<I18nlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [I18nlistComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(I18nlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
