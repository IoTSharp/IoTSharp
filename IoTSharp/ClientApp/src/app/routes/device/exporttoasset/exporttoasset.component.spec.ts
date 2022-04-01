import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExporttoassetComponent } from './exporttoasset.component';

describe('ExporttoassetComponent', () => {
  let component: ExporttoassetComponent;
  let fixture: ComponentFixture<ExporttoassetComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ExporttoassetComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ExporttoassetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
