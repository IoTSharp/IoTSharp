import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssetentitylistComponent } from './assetentitylist.component';

describe('AssetentitylistComponent', () => {
  let component: AssetentitylistComponent;
  let fixture: ComponentFixture<AssetentitylistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AssetentitylistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AssetentitylistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
