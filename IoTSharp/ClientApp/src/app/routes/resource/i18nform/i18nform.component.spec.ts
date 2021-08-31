import { ComponentFixture, TestBed } from '@angular/core/testing';

import { I18nformComponent } from './i18nform.component';

describe('I18nformComponent', () => {
  let component: I18nformComponent;
  let fixture: ComponentFixture<I18nformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [I18nformComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(I18nformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
