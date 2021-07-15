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
import { DesignerComponent } from './device/designer/designer.component';
import { UserlistComponent } from './user/userlist/userlist.component';
import { UserformComponent } from './user/userform/userform.component';
import { DevicegraphComponent } from './device/devicegraph/devicegraph.component';
import { PropformComponent } from './device/propform/propform.component';

const COMPONENTS: Type<null>[] = [];

@NgModule({
  imports: [SharedModule, RouteRoutingModule],
  declarations: [
    ...COMPONENTS,
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
  ],
})
export class RoutesModule {}
