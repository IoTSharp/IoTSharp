import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WidgettenantComponent } from './widgettenant.component';

describe('WidgettenantComponent', () => {
  let component: WidgettenantComponent;
  let fixture: ComponentFixture<WidgettenantComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [WidgettenantComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(WidgettenantComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
