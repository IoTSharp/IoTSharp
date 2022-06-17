import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FloweventviewComponent } from './floweventview.component';

describe('FloweventviewComponent', () => {
  let component: FloweventviewComponent;
  let fixture: ComponentFixture<FloweventviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FloweventviewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FloweventviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
