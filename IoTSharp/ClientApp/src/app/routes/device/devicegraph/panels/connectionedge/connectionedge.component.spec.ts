import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConnectionedgeComponent } from './connectionedge.component';

describe('ConnectionedgeComponent', () => {
  let component: ConnectionedgeComponent;
  let fixture: ComponentFixture<ConnectionedgeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConnectionedgeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ConnectionedgeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
