import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProduceformComponent } from './produceform.component';

describe('ProduceformComponent', () => {
  let component: ProduceformComponent;
  let fixture: ComponentFixture<ProduceformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProduceformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProduceformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
