import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewdeviceComponent } from './newdevice.component';

describe('NewdeviceComponent', () => {
  let component: NewdeviceComponent;
  let fixture: ComponentFixture<NewdeviceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NewdeviceComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NewdeviceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
