import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AlarmlistComponent } from './alarmlist.component';

describe('AlarmlistComponent', () => {
  let component: AlarmlistComponent;
  let fixture: ComponentFixture<AlarmlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AlarmlistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AlarmlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
