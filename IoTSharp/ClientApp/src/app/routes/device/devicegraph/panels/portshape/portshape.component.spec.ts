import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortshapeComponent } from './portshape.component';

describe('PortshapeComponent', () => {
  let component: PortshapeComponent;
  let fixture: ComponentFixture<PortshapeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PortshapeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PortshapeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
