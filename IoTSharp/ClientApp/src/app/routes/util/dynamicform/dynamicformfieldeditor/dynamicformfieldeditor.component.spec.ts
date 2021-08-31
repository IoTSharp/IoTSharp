import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicformfieldeditorComponent } from './dynamicformfieldeditor.component';

describe('DynamicformfieldeditorComponent', () => {
  let component: DynamicformfieldeditorComponent;
  let fixture: ComponentFixture<DynamicformfieldeditorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DynamicformfieldeditorComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamicformfieldeditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
