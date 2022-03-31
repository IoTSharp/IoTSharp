import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssetformComponent } from './assetform.component';

describe('AssetformComponent', () => {
  let component: AssetformComponent;
  let fixture: ComponentFixture<AssetformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AssetformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AssetformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
