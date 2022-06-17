import { NgModule } from '@angular/core';

import { DeviceformComponent } from './deviceform/deviceform.component';
import { DevicelistComponent } from './devicelist/devicelist.component';
import { DevicepropComponent } from './deviceprop/deviceprop.component';
import { DevicetokendialogComponent } from './devicetokendialog/devicetokendialog.component';
import { PropformComponent } from './propform/propform.component';
import { SharedModule } from '@shared';
import { ExporttoassetComponent } from './exporttoasset/exporttoasset.component';
import { RulesdownlinkComponent } from './rulesdownlink/rulesdownlink.component';
import { ConnectionedgeComponent } from './devicegraph/panels/connectionedge/connectionedge.component';
import { DevivceshapeComponent } from './devicegraph/panels/devivceshape/devivceshape.component';
import { GatewayshapeComponent } from './devicegraph/panels/gatewayshape/gatewayshape.component';
import { PortshapeComponent } from './devicegraph/panels/portshape/portshape.component';
import { DevicesRoutingModule } from './devices-routing.module';
import { CommonDialogSevice } from '../util/commonDialogSevice';
import { NgxEchartsModule } from 'ngx-echarts';
import { DevicegraphComponent } from './devicegraph/devicegraph.component';
import { ClipboardModule } from 'ngx-clipboard';
import { WidgetsModule } from '../widgets/widgets.module';
import { NzIconModule } from 'ng-zorro-antd/icon';
const COMPONENTS = [
  DeviceformComponent,
  DevicelistComponent,
  DevicepropComponent,
  DevicetokendialogComponent,
  PropformComponent,
  ExporttoassetComponent,
  RulesdownlinkComponent,
  ConnectionedgeComponent,
  DevivceshapeComponent,
  GatewayshapeComponent,
  PortshapeComponent,
  DevicegraphComponent
];

@NgModule({
  imports: [
    DevicesRoutingModule,
    SharedModule,
    NgxEchartsModule.forRoot({
      echarts: () => import('echarts')
    }),
    ClipboardModule,
    WidgetsModule,
    NzIconModule
  ],
  providers: [CommonDialogSevice],
  declarations: COMPONENTS
})
export class DevicesModule {}
