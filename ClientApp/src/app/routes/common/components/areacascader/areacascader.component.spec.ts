import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AreacascaderComponent } from './areacascader.component';

describe('AreacascaderComponent', () => {
  let component: AreacascaderComponent;
  let fixture: ComponentFixture<AreacascaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AreacascaderComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AreacascaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
