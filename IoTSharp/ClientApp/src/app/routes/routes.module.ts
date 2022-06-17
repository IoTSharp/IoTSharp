import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';
import { RouteRoutingModule } from './routes-routing.module';
import { SlidecontrolComponent } from './widgets/slidecontrol/slidecontrol.component';

const COMPONENTS: Array<Type<null>> = [];

@NgModule({
  imports: [SharedModule, RouteRoutingModule],
  declarations: [...COMPONENTS]
})
export class RoutesModule {}
