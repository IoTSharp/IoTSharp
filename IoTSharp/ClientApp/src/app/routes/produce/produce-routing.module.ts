import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProducelistComponent } from './producelist/producelist.component';


const routes: Routes = [
  { path: 'producelist', component: ProducelistComponent },

];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProduceRoutingModule {}
