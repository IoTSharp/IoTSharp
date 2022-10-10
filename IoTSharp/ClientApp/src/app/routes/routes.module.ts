import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';
import { RouteRoutingModule } from './routes-routing.module';
import { SlidecontrolComponent } from './widgets/slidecontrol/slidecontrol.component';
import { ProducelistComponent } from './produce/producelist/producelist.component';
import { ProduceformComponent } from './produce/produceform/produceform.component';

const COMPONENTS: Array<Type<null>> = [];

@NgModule({
  imports: [SharedModule, RouteRoutingModule],
  declarations: [...COMPONENTS, ProducelistComponent, ProduceformComponent]
})
export class RoutesModule {}
