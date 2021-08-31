import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TenantlistComponent } from './tenantlist.component';

describe('TenantlistComponent', () => {
  let component: TenantlistComponent;
  let fixture: ComponentFixture<TenantlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TenantlistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TenantlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
