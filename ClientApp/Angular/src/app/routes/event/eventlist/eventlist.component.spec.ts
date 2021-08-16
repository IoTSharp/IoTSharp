import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EventlistComponent } from './eventlist.component';

describe('EventlistComponent', () => {
  let component: EventlistComponent;
  let fixture: ComponentFixture<EventlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EventlistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EventlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
