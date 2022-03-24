import { NgModule, Type } from '@angular/core';
import { G2BarModule } from '@delon/chart/bar';
import { G2GaugeModule } from '@delon/chart/gauge';
import { SharedModule } from '@shared';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { ClipboardModule } from 'ngx-clipboard';
import { DevicecertificateComponent } from './device/devicecertificate/devicecertificate.component';
import { DeviceformComponent } from './device/deviceform/deviceform.component';
import { DevicegraphComponent } from './device/devicegraph/devicegraph.component';
import { DevicelistComponent } from './device/devicelist/devicelist.component';
import { ParampartComponent } from './device/deviceparam/parampart/parampart.component';
import { ProppartComponent } from './device/deviceprop/proppart/proppart.component';
import { DevicesceneComponent } from './device/devicescene/devicescene.component';
import { DevicetokendialogComponent } from './device/devicetokendialog/devicetokendialog.component';
import { PropformComponent } from './device/propform/propform.component';
import { RulesdownlinkComponent } from './device/rulesdownlink/rulesdownlink.component';
import { DictionaryformComponent } from './dictionary/dictionaryform/dictionaryform.component';
import { DictionarygroupformComponent } from './dictionary/dictionarygroupform/dictionarygroupform.component';
import { DictionarygrouplistComponent } from './dictionary/dictionarygrouplist/dictionarygrouplist.component';
import { DictionarylistComponent } from './dictionary/dictionarylist/dictionarylist.component';
import { EventlistComponent } from './event/eventlist/eventlist.component';
import { ComponentlistComponent } from './flow/componentlist/componentlist.component';
import { DesignerComponent } from './flow/designer/designer.component';
import { DiagramComponent } from './flow/diagram/diagram.component';
import { FloweventsComponent } from './flow/flowevents/flowevents.component';
import { FloweventviewComponent } from './flow/floweventview/floweventview.component';
import { FlowformComponent } from './flow/flowform/flowform.component';
import { FlowlistComponent } from './flow/flowlist/flowlist.component';
import { ForkdialogComponent } from './flow/forkdialog/forkdialog.component';
import { ScriptlistComponent } from './flow/scriptlist/scriptlist.component';
import { SequenceflowtesterComponent } from './flow/sequenceflowtester/sequenceflowtester.component';
import { SubscriptionbindformComponent } from './flow/subscriptionbindform/subscriptionbindform.component';
import { SubscriptionformComponent } from './flow/subscriptionform/subscriptionform.component';
import { SubscriptionlistComponent } from './flow/subscriptionlist/subscriptionlist.component';
import { TaskexecutorformComponent } from './flow/taskexecutorform/taskexecutorform.component';
import { TaskexecutorlistComponent } from './flow/taskexecutorlist/taskexecutorlist.component';
import { TasktesterComponent } from './flow/tasktester/tasktester.component';
import { I18nformComponent } from './resource/i18nform/i18nform.component';
import { I18nlistComponent } from './resource/i18nlist/i18nlist.component';

import { RouteRoutingModule } from './routes-routing.module';
import { CertmgrComponent } from './setttings/certmgr/certmgr.component';
import { TenantformComponent } from './tenant/tenantform/tenantform.component';
import { TenantlistComponent } from './tenant/tenantlist/tenantlist.component';
import { UserformComponent } from './user/userform/userform.component';
import { UserlistComponent } from './user/userlist/userlist.component';
import { CodeviewComponent } from './util/code/codeview/codeview.component';
import { CodefieldComponent } from './util/codefield/codefield.component';
import { ConditionbuilderComponent } from './util/dynamicform/conditionbuilder/conditionbuilder.component';
import { controldirective } from './util/dynamicform/controldirective';
import { TextBoxComponent } from './util/dynamicform/cps/text-box/text-box.component';
import { DynamicformdesignerComponent } from './util/dynamicform/dynamicformdesigner/dynamicformdesigner.component';
import { Dynamicformdesignerv2Component } from './util/dynamicform/dynamicformdesignerv2/dynamicformdesignerv2.component';
import { DynamicformeditorComponent } from './util/dynamicform/dynamicformeditor/dynamicformeditor.component';
import { DynamicformfieldeditorComponent } from './util/dynamicform/dynamicformfieldeditor/dynamicformfieldeditor.component';
import { DynamicformlistComponent } from './util/dynamicform/dynamicformlist/dynamicformlist.component';
import { DynamicformresultviewComponent } from './util/dynamicform/dynamicformresultview/dynamicformresultview.component';
import { DynamicformtesterComponent } from './util/dynamicform/dynamicformtester/dynamicformtester.component';
import { DynamicformviewComponent } from './util/dynamicform/dynamicformview/dynamicformview.component';
import { DynamicpartComponent } from './util/dynamicform/dynamicpart/dynamicpart.component';
import { FieldpartComponent } from './util/dynamicform/fieldpart/fieldpart.component';
import { fielddirective } from './util/dynamicform/fieldpartdirective';
import { SearchformgeneratorComponent } from './util/dynamicform/searchformgenerator/searchformgenerator.component';
import { DynamictablecolumeditorComponent } from './util/dynamictable/dynamictablecolumeditor/dynamictablecolumeditor.component';
import { DynamictablelistComponent } from './util/dynamictable/dynamictablelist/dynamictablelist.component';
import { DynamictabletesterComponent } from './util/dynamictable/dynamictabletester/dynamictabletester.component';
import { DynamictableviewComponent } from './util/dynamictable/dynamictableview/dynamictableview.component';
import { FlowsimulatorComponent } from './util/flow/flowsimulator/flowsimulator.component';
import { WidgetsModule } from './widgets/widgets.module';
import { DevicemodellistComponent } from './devicemodel/devicemodellist/devicemodellist.component';
import { DevicemodelformComponent } from './devicemodel/devicemodelform/devicemodelform.component';
import { DevicemodelcommandComponent } from './devicemodel/devicemodelcommand/devicemodelcommand.component';
import { ConnectionedgeComponent } from './device/devicegraph/panels/connectionedge/connectionedge.component';
import { DevivceshapeComponent } from './device/devicegraph/panels/devivceshape/devivceshape.component';
import { GatewayshapeComponent } from './device/devicegraph/panels/gatewayshape/gatewayshape.component';
import { PortshapeComponent } from './device/devicegraph/panels/portshape/portshape.component';
import { toolpaneldirective } from './device/devicegraph/panels/toolpaneldirective';
import { NgxEchartsModule } from 'ngx-echarts';

const COMPONENTS: Array<Type<null>> = [];
const Directive: Type<void>[] = [fielddirective, controldirective, toolpaneldirective];
@NgModule({
  imports: [SharedModule, RouteRoutingModule, G2BarModule, G2GaugeModule, NzIconModule, WidgetsModule, ClipboardModule, NgxEchartsModule.forRoot({
    echarts: () => import('echarts'),
  }),],
  declarations: [
    ...COMPONENTS,

    ...Directive,
    TenantlistComponent,
    TenantformComponent,
    DeviceformComponent,
    DevicelistComponent,
    ParampartComponent,
    ProppartComponent,
    DesignerComponent,
    UserlistComponent,
    UserformComponent,
    DevicegraphComponent,
    PropformComponent,
    DevicesceneComponent,
    FlowlistComponent,
    FlowformComponent,
    DiagramComponent,
    DynamicformlistComponent,
    DynamicformeditorComponent,
    DynamicformdesignerComponent,
    DynamicformtesterComponent,
    DynamicformfieldeditorComponent,
    FlowsimulatorComponent,
    FieldpartComponent,
    DictionarygrouplistComponent,
    DictionarylistComponent,
    DictionaryformComponent,
    DictionarygroupformComponent,
    I18nlistComponent,
    I18nformComponent,
    DynamicformresultviewComponent,
    DynamicformviewComponent,
    CodeviewComponent,
    CodefieldComponent,
    EventlistComponent,
    DynamictablecolumeditorComponent,
    DynamictableviewComponent,
    DynamictabletesterComponent,
    DynamictablelistComponent,
    Dynamicformdesignerv2Component,
    DynamicpartComponent,
    TextBoxComponent,
    RulesdownlinkComponent,
    DevicecertificateComponent,
    DevicetokendialogComponent,
    ScriptlistComponent,
    ComponentlistComponent,
    FloweventsComponent,
    FloweventviewComponent,
    TaskexecutorlistComponent,
    TaskexecutorformComponent,
    SequenceflowtesterComponent,
    TasktesterComponent,
    ForkdialogComponent,
    CertmgrComponent,
    SubscriptionlistComponent,
    SubscriptionformComponent,
    SubscriptionbindformComponent,
    SearchformgeneratorComponent,
    ConditionbuilderComponent,
    DevicemodellistComponent,
    DevicemodelformComponent,
    DevicemodelcommandComponent,
    ConnectionedgeComponent,
    DevivceshapeComponent,
    GatewayshapeComponent,
    PortshapeComponent
  ]
})
export class RoutesModule {}
