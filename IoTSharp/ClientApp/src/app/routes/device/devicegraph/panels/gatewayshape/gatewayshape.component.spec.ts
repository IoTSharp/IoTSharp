import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GatewayshapeComponent } from './gatewayshape.component';

describe('GatewayshapeComponent', () => {
  let component: GatewayshapeComponent;
  let fixture: ComponentFixture<GatewayshapeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GatewayshapeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GatewayshapeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
