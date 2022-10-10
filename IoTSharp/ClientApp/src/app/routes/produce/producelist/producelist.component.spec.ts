import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProducelistComponent } from './producelist.component';

describe('ProducelistComponent', () => {
  let component: ProducelistComponent;
  let fixture: ComponentFixture<ProducelistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProducelistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProducelistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
