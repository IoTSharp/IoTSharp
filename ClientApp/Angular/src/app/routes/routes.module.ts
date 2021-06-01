import { NgModule, Type } from '@angular/core';

import { SharedModule } from '@shared';
import { RouteRoutingModule } from './routes-routing.module';
import { TenantlistComponent } from './tenant/tenantlist/tenantlist.component';
import { TenantformComponent } from './tenant/tenantform/tenantform.component';
import { CustomerlistComponent } from './customer/customerlist/customerlist.component';
import { CustomerformComponent } from './customer/customerform/customerform.component';
import { DeviceformComponent } from './device/deviceform/deviceform.component';
import { DevicelistComponent } from './device/devicelist/devicelist.component';

const COMPONENTS: Type<null>[] = [];

@NgModule({
  imports: [SharedModule, RouteRoutingModule],
  declarations: [...COMPONENTS, TenantlistComponent, TenantformComponent, CustomerlistComponent, CustomerformComponent, DeviceformComponent, DevicelistComponent],
})
export class RoutesModule {}
