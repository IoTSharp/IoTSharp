import { NgModule } from '@angular/core';
import { SharedModule } from '@shared';
import { ProduceRoutingModule } from './produce-routing.module';
import { ProducedatadictionaryformComponent } from './producedatadictionaryform/producedatadictionaryform.component';
import { ProducedataformComponent } from './producedataform/producedataform.component';


const COMPONENTS = [
  ProducedataformComponent,ProducedatadictionaryformComponent
];

@NgModule({
  imports: [ProduceRoutingModule, SharedModule],
  providers: [],
  declarations: COMPONENTS
})
export class ProduceModule {}
