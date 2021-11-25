import { NgModule, Type } from '@angular/core';
import { G2MiniAreaModule } from '@delon/chart/mini-area';
import { G2MiniBarModule } from '@delon/chart/mini-bar';
import { DelonFormModule } from '@delon/form';
import { SharedModule } from '@shared';
import { NzCarouselModule } from 'ng-zorro-antd/carousel';
import { FlowviewerComponent } from './flowviewer/flowviewer.component';

import { WidgetsRoutingModule } from './widgets-routing.module';
import { WidgetsComponent } from './widgets/widgets.component';

const COMPONENTS: Type<void>[] = [WidgetsComponent, FlowviewerComponent];

@NgModule({
  imports: [SharedModule, WidgetsRoutingModule, NzCarouselModule, G2MiniBarModule, G2MiniAreaModule, DelonFormModule],
  declarations: COMPONENTS,
  exports: COMPONENTS
})
export class WidgetsModule {}
