import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TenantformComponent } from './tenantform.component';

describe('TenantformComponent', () => {
  let component: TenantformComponent;
  let fixture: ComponentFixture<TenantformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TenantformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TenantformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
