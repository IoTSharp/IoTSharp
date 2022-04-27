import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MqtttesterComponent } from './mqtttester.component';

describe('MqtttesterComponent', () => {
  let component: MqtttesterComponent;
  let fixture: ComponentFixture<MqtttesterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MqtttesterComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MqtttesterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
