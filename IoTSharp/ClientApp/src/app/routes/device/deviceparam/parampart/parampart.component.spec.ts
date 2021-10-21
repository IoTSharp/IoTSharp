import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ParampartComponent } from './parampart.component';

describe('ParampartComponent', () => {
  let component: ParampartComponent;
  let fixture: ComponentFixture<ParampartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ParampartComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ParampartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
