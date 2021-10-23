import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ComponentlistComponent } from './componentlist.component';

describe('ComponentlistComponent', () => {
  let component: ComponentlistComponent;
  let fixture: ComponentFixture<ComponentlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ComponentlistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ComponentlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
