import { NgModule } from '@angular/core';
import { SharedModule } from '@shared';
import { CertmgrComponent } from './certmgr/certmgr.component';
import { CustomerformComponent } from './customerform/customerform.component';
import { CustomerlistComponent } from './customerlist/customerlist.component';
import { DictionaryformComponent } from './dictionaryform/dictionaryform.component';
import { DictionarygroupformComponent } from './dictionarygroupform/dictionarygroupform.component';
import { DictionarygrouplistComponent } from './dictionarygrouplist/dictionarygrouplist.component';
import { DictionarylistComponent } from './dictionarylist/dictionarylist.component';
import { I18nformComponent } from './i18nform/i18nform.component';
import { I18nlistComponent } from './i18nlist/i18nlist.component';
import { SettingsRoutingModule } from './settings-routing.module';
import { TenantformComponent } from './tenantform/tenantform.component';
import { TenantlistComponent } from './tenantlist/tenantlist.component';
import { UserformComponent } from './userform/userform.component';
import { UserlistComponent } from './userlist/userlist.component';

const COMPONENTS = [
  CertmgrComponent,
  CustomerformComponent,
  CustomerformComponent,
  CustomerlistComponent,
  DictionaryformComponent,
  DictionarygroupformComponent,
  DictionarygrouplistComponent,
  DictionarylistComponent,
  I18nformComponent,
  I18nlistComponent,
  TenantformComponent,
  TenantlistComponent,
  UserformComponent,
  UserlistComponent
];

@NgModule({
  imports: [SettingsRoutingModule, SharedModule],
  providers: [],
  declarations: COMPONENTS
})
export class SettingsModule {}
