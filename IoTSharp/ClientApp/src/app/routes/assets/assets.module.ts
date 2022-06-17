import { NgModule } from '@angular/core';
import { SharedModule } from '@shared';
import { CommonDialogSevice } from '../util/commonDialogSevice';
import { AssetentityformComponent } from './assetentityform/assetentityform.component';
import { AssetentitylistComponent } from './assetentitylist/assetentitylist.component';
import { AssetformComponent } from './assetform/assetform.component';
import { AssetlistComponent } from './assetlist/assetlist.component';
import { AssetrelationformComponent } from './assetrelationform/assetrelationform.component';
import { AssetsRoutingModule } from './assets-routing.module';

const COMPONENTS = [AssetlistComponent, AssetformComponent, AssetentitylistComponent, AssetentityformComponent, AssetrelationformComponent];

@NgModule({
  imports: [AssetsRoutingModule, SharedModule],
  providers:[CommonDialogSevice],
  declarations: COMPONENTS
})
export class AssetsModule {}
