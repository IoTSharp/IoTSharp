import { NgModule } from '@angular/core';
import { SharedModule } from '@shared';
import { NgxEchartsModule } from 'ngx-echarts';
import { CommonDialogSevice } from '../util/commonDialogSevice';
import { WidgetsModule } from '../widgets/widgets.module';
import { ConditionbuilderComponent } from './conditionbuilder/conditionbuilder.component';
import { DesignerComponent } from './designer/designer.component';
import { DiagramComponent } from './diagram/diagram.component';
import { FloweventsComponent } from './flowevents/flowevents.component';
import { FloweventviewComponent } from './floweventview/floweventview.component';
import { FlowformComponent } from './flowform/flowform.component';
import { FlowlistComponent } from './flowlist/flowlist.component';
import { ForkdialogComponent } from './forkdialog/forkdialog.component';
import { RuledeviceComponent } from './ruledevice/ruledevice.component';
import { RulesRoutingModule } from './rules-routing.module';
import { SequenceflowtesterComponent } from './sequenceflowtester/sequenceflowtester.component';
import { TasktesterComponent } from './tasktester/tasktester.component';

const COMPONENTS = [
  DesignerComponent,
  DiagramComponent,
  FloweventsComponent,
  FloweventviewComponent,
  FlowformComponent,
  FlowlistComponent,
  ForkdialogComponent,
  SequenceflowtesterComponent,
  TasktesterComponent,
  RuledeviceComponent,ConditionbuilderComponent
  
];

@NgModule({
  imports: [
    RulesRoutingModule,
    SharedModule,
    WidgetsModule,
    NgxEchartsModule.forRoot({
      echarts: () => import('echarts')
    })
  ],
  providers: [CommonDialogSevice],
  declarations: COMPONENTS
})
export class RulesModule {}
