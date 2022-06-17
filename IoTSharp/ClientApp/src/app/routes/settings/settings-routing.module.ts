import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CertmgrComponent } from './certmgr/certmgr.component';
import { CustomerlistComponent } from './customerlist/customerlist.component';
import { DictionarygrouplistComponent } from './dictionarygrouplist/dictionarygrouplist.component';
import { DictionarylistComponent } from './dictionarylist/dictionarylist.component';
import { I18nlistComponent } from './i18nlist/i18nlist.component';
import { TenantlistComponent } from './tenantlist/tenantlist.component';
import { UserlistComponent } from './userlist/userlist.component';

const routes: Routes = [
  { path: 'i18nlist', component: I18nlistComponent },
  { path: 'userlist', component: UserlistComponent },
  { path: 'tenantlist', component: TenantlistComponent },
  { path: 'dictionarylist', component: DictionarylistComponent },
  { path: 'customerlist', component: CustomerlistComponent },
  { path: 'dictionarygrouplist', component: DictionarygrouplistComponent },
  { path: 'certmgr', component: CertmgrComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SettingsRoutingModule {}
