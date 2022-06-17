import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AssetentitylistComponent } from './assetentitylist/assetentitylist.component';
import { AssetlistComponent } from './assetlist/assetlist.component';
const routes: Routes = [
  { path: 'assetlist', component: AssetlistComponent },
  { path: 'assetentitylist', component: AssetentitylistComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AssetsRoutingModule {}
