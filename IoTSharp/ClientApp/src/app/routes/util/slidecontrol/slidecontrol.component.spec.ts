import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SlidecontrolComponent } from './slidecontrol.component';

describe('SlidecontrolComponent', () => {
  let component: SlidecontrolComponent;
  let fixture: ComponentFixture<SlidecontrolComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SlidecontrolComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SlidecontrolComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
