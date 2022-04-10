import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssetrelationformComponent } from './assetrelationform.component';

describe('AssetrelationformComponent', () => {
  let component: AssetrelationformComponent;
  let fixture: ComponentFixture<AssetrelationformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AssetrelationformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AssetrelationformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
