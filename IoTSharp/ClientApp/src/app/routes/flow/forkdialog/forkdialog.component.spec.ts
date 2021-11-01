import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ForkdialogComponent } from './forkdialog.component';

describe('ForkdialogComponent', () => {
  let component: ForkdialogComponent;
  let fixture: ComponentFixture<ForkdialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ForkdialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ForkdialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
