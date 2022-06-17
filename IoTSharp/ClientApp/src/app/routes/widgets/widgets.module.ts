import { NgModule, Type } from '@angular/core';
import { TagSelectModule } from '@delon/abc/tag-select';
import { G2BarModule } from '@delon/chart/bar';
import { G2GaugeModule } from '@delon/chart/gauge';
import { G2MiniAreaModule } from '@delon/chart/mini-area';
import { G2MiniBarModule } from '@delon/chart/mini-bar';
import { G2TimelineModule } from '@delon/chart/timeline';
import { DelonFormModule } from '@delon/form';
import { SharedModule } from '@shared';
import { NzCarouselModule } from 'ng-zorro-antd/carousel';
import { FlowviewerComponent } from './flowviewer/flowviewer.component';
import { WidgetdeviceComponent } from './widgetdevice/widgetdevice.component';
import { WidgetsRoutingModule } from './widgets-routing.module';
import { WidgetsComponent } from './widgets/widgets.component';
import { NgxEchartsModule } from 'ngx-echarts';
import { FlowsimulatorComponent } from './flowsimulator/flowsimulator.component';
import { SlidecontrolComponent } from './slidecontrol/slidecontrol.component';
import { CodeFieldComponent } from './parts/code-field/code-field.component';
import { toolpaneldirective } from 'src/app/models/devicegraph/toolpaneldirective';

const COMPONENTS: Type<void>[] = [
  WidgetsComponent,
  FlowviewerComponent,
  WidgetdeviceComponent,
  FlowsimulatorComponent,
  SlidecontrolComponent
];
const Directives: Type<void>[] = [toolpaneldirective];
const FormWidgets: Type<void>[] = [CodeFieldComponent];
@NgModule({
  imports: [
    WidgetsRoutingModule,
    SharedModule,
    NzCarouselModule,
    G2BarModule,
    G2GaugeModule,
    G2MiniBarModule,
    G2MiniAreaModule,
    DelonFormModule,
    G2TimelineModule,
    TagSelectModule,
    NgxEchartsModule.forRoot({
      echarts: () => import('echarts')
    })
  ],
  exports: [...COMPONENTS, ...FormWidgets, ...Directives],
  declarations: [...COMPONENTS, ...FormWidgets, ...Directives]
})
export class WidgetsModule {}
