import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RulesdownlinkComponent } from './rulesdownlink.component';

describe('RulesdownlinkComponent', () => {
  let component: RulesdownlinkComponent;
  let fixture: ComponentFixture<RulesdownlinkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RulesdownlinkComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RulesdownlinkComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
