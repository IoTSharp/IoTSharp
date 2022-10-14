import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProducedatadictionaryformComponent } from './producedatadictionaryform.component';

describe('ProducedatadictionaryformComponent', () => {
  let component: ProducedatadictionaryformComponent;
  let fixture: ComponentFixture<ProducedatadictionaryformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProducedatadictionaryformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProducedatadictionaryformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
