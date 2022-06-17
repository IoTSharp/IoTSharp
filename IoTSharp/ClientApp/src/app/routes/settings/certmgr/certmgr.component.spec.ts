import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CertmgrComponent } from './certmgr.component';

describe('CertmgrComponent', () => {
  let component: CertmgrComponent;
  let fixture: ComponentFixture<CertmgrComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CertmgrComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CertmgrComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
