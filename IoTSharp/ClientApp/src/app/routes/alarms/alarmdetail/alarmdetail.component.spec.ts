import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AlarmdetailComponent } from './alarmdetail.component';

describe('AlarmdetailComponent', () => {
  let component: AlarmdetailComponent;
  let fixture: ComponentFixture<AlarmdetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AlarmdetailComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AlarmdetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
