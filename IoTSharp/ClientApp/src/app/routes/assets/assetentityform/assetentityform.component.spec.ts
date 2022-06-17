import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssetentityformComponent } from './assetentityform.component';

describe('AssetentityformComponent', () => {
  let component: AssetentityformComponent;
  let fixture: ComponentFixture<AssetentityformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AssetentityformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AssetentityformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
