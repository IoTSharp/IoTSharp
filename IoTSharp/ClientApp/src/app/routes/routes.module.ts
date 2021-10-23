import { NgModule, Type } from '@angular/core';

import { SharedModule } from '@shared';
import { RouteRoutingModule } from './routes-routing.module';
import { TenantlistComponent } from './tenant/tenantlist/tenantlist.component';
import { TenantformComponent } from './tenant/tenantform/tenantform.component';
import { CustomerlistComponent } from './customer/customerlist/customerlist.component';
import { CustomerformComponent } from './customer/customerform/customerform.component';
import { DeviceformComponent } from './device/deviceform/deviceform.component';
import { DevicelistComponent } from './device/devicelist/devicelist.component';
import { ParampartComponent } from './device/deviceparam/parampart/parampart.component';
import { ProppartComponent } from './device/deviceprop/proppart/proppart.component';

import { UserlistComponent } from './user/userlist/userlist.component';
import { UserformComponent } from './user/userform/userform.component';
import { DevicegraphComponent } from './device/devicegraph/devicegraph.component';
import { PropformComponent } from './device/propform/propform.component';
import { DevicesceneComponent } from './device/devicescene/devicescene.component';
import { FlowlistComponent } from './flow/flowlist/flowlist.component';
import { FlowformComponent } from './flow/flowform/flowform.component';
import { DiagramComponent } from './flow/diagram/diagram.component';
import { DesignerComponent } from './flow/designer/designer.component';
import { DynamicformlistComponent } from './util/dynamicform/dynamicformlist/dynamicformlist.component';
import { DynamicformeditorComponent } from './util/dynamicform/dynamicformeditor/dynamicformeditor.component';
import { DynamicformdesignerComponent } from './util/dynamicform/dynamicformdesigner/dynamicformdesigner.component';
import { DynamicformtesterComponent } from './util/dynamicform/dynamicformtester/dynamicformtester.component';
import { DynamicformfieldeditorComponent } from './util/dynamicform/dynamicformfieldeditor/dynamicformfieldeditor.component';
import { FlowsimulatorComponent } from './util/flow/flowsimulator/flowsimulator.component';
import { FieldpartComponent } from './util/dynamicform/fieldpart/fieldpart.component';
import { DictionarygrouplistComponent } from './dictionary/dictionarygrouplist/dictionarygrouplist.component';
import { DictionarylistComponent } from './dictionary/dictionarylist/dictionarylist.component';
import { DictionaryformComponent } from './dictionary/dictionaryform/dictionaryform.component';
import { DictionarygroupformComponent } from './dictionary/dictionarygroupform/dictionarygroupform.component';
import { I18nlistComponent } from './resource/i18nlist/i18nlist.component';
import { I18nformComponent } from './resource/i18nform/i18nform.component';
import { fielddirective } from './util/dynamicform/fieldpartdirective';
import { DynamicformresultviewComponent } from './util/dynamicform/dynamicformresultview/dynamicformresultview.component';
import { DynamicformviewComponent } from './util/dynamicform/dynamicformview/dynamicformview.component';
import { WidgetsModule } from './widgets/widgets.module';
import { CodeviewComponent } from './util/code/codeview/codeview.component';
import { DelonFormModule, WidgetRegistry } from '@delon/form';

import { CodefieldComponent } from './util/codefield/codefield.component';
import { EventlistComponent } from './event/eventlist/eventlist.component';
import { DynamictablecolumeditorComponent } from './util/dynamictable/dynamictablecolumeditor/dynamictablecolumeditor.component';
import { DynamictableviewComponent } from './util/dynamictable/dynamictableview/dynamictableview.component';
import { DynamictabletesterComponent } from './util/dynamictable/dynamictabletester/dynamictabletester.component';
import { DynamictablelistComponent } from './util/dynamictable/dynamictablelist/dynamictablelist.component';
import { Dynamicformdesignerv2Component } from './util/dynamicform/dynamicformdesignerv2/dynamicformdesignerv2.component';
import { DynamicpartComponent } from './util/dynamicform/dynamicpart/dynamicpart.component';

import { TextBoxComponent } from './util/dynamicform/cps/text-box/text-box.component';
import { controldirective } from './util/dynamicform/controldirective';
import { RulesdownlinkComponent } from './device/rulesdownlink/rulesdownlink.component';

import { DevicecertificateComponent } from './device/devicecertificate/devicecertificate.component';

const COMPONENTS: Type<null>[] = [];
const Directive: Type<void>[] = [fielddirective, controldirective];
@NgModule({
  imports: [SharedModule, RouteRoutingModule, WidgetsModule, DelonFormModule.forRoot()],
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
  ],
})
export class RoutesModule {
  constructor(widgetRegistry: WidgetRegistry) {
    widgetRegistry.register(CodefieldComponent.KEY, CodefieldComponent);
  }
}
