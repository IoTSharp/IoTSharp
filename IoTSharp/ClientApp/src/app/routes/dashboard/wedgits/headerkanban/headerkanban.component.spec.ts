import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HeaderkanbanComponent } from './headerkanban.component';

describe('HeaderkanbanComponent', () => {
  let component: HeaderkanbanComponent;
  let fixture: ComponentFixture<HeaderkanbanComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ HeaderkanbanComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(HeaderkanbanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
