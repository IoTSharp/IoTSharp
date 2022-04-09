import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FlowdevicelistComponent } from './flowdevicelist.component';

describe('FlowdevicelistComponent', () => {
  let component: FlowdevicelistComponent;
  let fixture: ComponentFixture<FlowdevicelistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FlowdevicelistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FlowdevicelistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
