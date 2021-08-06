import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CodeviewComponent } from './codeview.component';

describe('CodeviewComponent', () => {
  let component: CodeviewComponent;
  let fixture: ComponentFixture<CodeviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CodeviewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CodeviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
