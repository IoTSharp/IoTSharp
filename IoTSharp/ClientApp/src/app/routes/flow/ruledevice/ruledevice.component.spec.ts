import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RuledeviceComponent } from './ruledevice.component';

describe('RuledeviceComponent', () => {
  let component: RuledeviceComponent;
  let fixture: ComponentFixture<RuledeviceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RuledeviceComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RuledeviceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
