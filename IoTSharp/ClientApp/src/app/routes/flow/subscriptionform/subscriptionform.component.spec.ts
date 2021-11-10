import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SubscriptionformComponent } from './subscriptionform.component';

describe('SubscriptionformComponent', () => {
  let component: SubscriptionformComponent;
  let fixture: ComponentFixture<SubscriptionformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SubscriptionformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SubscriptionformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
