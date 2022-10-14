import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProducedataformComponent } from './producedataform.component';

describe('ProducedataformComponent', () => {
  let component: ProducedataformComponent;
  let fixture: ComponentFixture<ProducedataformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProducedataformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProducedataformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
