import { NgModule } from '@angular/core';
import { SharedModule } from '@shared';
import { ProduceRoutingModule } from './produce-routing.module';


const COMPONENTS = [

];

@NgModule({
  imports: [ProduceRoutingModule, SharedModule],
  providers: [],
  declarations: COMPONENTS
})
export class SettingsModule {}
