import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreatedeviceformComponent } from './createdeviceform.component';

describe('CreatedeviceformComponent', () => {
  let component: CreatedeviceformComponent;
  let fixture: ComponentFixture<CreatedeviceformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CreatedeviceformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CreatedeviceformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
