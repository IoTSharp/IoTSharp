import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SubscriptionbindformComponent } from './subscriptionbindform.component';

describe('SubscriptionbindformComponent', () => {
  let component: SubscriptionbindformComponent;
  let fixture: ComponentFixture<SubscriptionbindformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SubscriptionbindformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SubscriptionbindformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
