import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SubscriptionlistComponent } from './subscriptionlist.component';

describe('SubscriptionlistComponent', () => {
  let component: SubscriptionlistComponent;
  let fixture: ComponentFixture<SubscriptionlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SubscriptionlistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SubscriptionlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
