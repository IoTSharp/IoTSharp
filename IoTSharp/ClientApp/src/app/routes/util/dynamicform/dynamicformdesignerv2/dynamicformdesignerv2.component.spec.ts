import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Dynamicformdesignerv2Component } from './dynamicformdesignerv2.component';

describe('Dynamicformdesignerv2Component', () => {
  let component: Dynamicformdesignerv2Component;
  let fixture: ComponentFixture<Dynamicformdesignerv2Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ Dynamicformdesignerv2Component ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(Dynamicformdesignerv2Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
