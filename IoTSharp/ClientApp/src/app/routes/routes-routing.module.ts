import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SimpleGuard } from '@delon/auth';
import { PreloadOptionalModules } from '@delon/theme';
import { environment } from '@env/environment';

// layout
import { LayoutBasicComponent } from '../layout/basic/basic.component';
import { LayoutBlankComponent } from '../layout/blank/blank.component';
import { DevicegraphComponent } from './device/devicegraph/devicegraph.component';
import { DevicelistComponent } from './device/devicelist/devicelist.component';
import { DevicesceneComponent } from './device/devicescene/devicescene.component';
import { DevicemodellistComponent } from './devicemodel/devicemodellist/devicemodellist.component';
import { DictionarygrouplistComponent } from './dictionary/dictionarygrouplist/dictionarygrouplist.component';
import { DictionarylistComponent } from './dictionary/dictionarylist/dictionarylist.component';
import { ComponentlistComponent } from './flow/componentlist/componentlist.component';
import { DesignerComponent } from './flow/designer/designer.component';
import { FloweventsComponent } from './flow/flowevents/flowevents.component';
import { FlowlistComponent } from './flow/flowlist/flowlist.component';
import { ScriptlistComponent } from './flow/scriptlist/scriptlist.component';
import { SubscriptionlistComponent } from './flow/subscriptionlist/subscriptionlist.component';
import { TaskexecutorlistComponent } from './flow/taskexecutorlist/taskexecutorlist.component';
import { I18nlistComponent } from './resource/i18nlist/i18nlist.component';
import { CertmgrComponent } from './setttings/certmgr/certmgr.component';
import { TenantlistComponent } from './tenant/tenantlist/tenantlist.component';
import { UserlistComponent } from './user/userlist/userlist.component';
import { CodeviewComponent } from './util/code/codeview/codeview.component';
import { DynamicformlistComponent } from './util/dynamicform/dynamicformlist/dynamicformlist.component';

const routes: Routes = [
  {
    path: '',
    component: LayoutBasicComponent,
    canActivate: [SimpleGuard],
    canActivateChild: [SimpleGuard],
    data: {},
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadChildren: () => import('./dashboard/dashboard.module').then(m => m.DashboardModule),
        data: { preload: true }
      },
      {
        path: 'widgets',
        loadChildren: () => import('./widgets/widgets.module').then(m => m.WidgetsModule)
      },
      { path: 'style', loadChildren: () => import('./style/style.module').then(m => m.StyleModule) },
      { path: 'delon', loadChildren: () => import('./delon/delon.module').then(m => m.DelonModule) },
      { path: 'extras', loadChildren: () => import('./extras/extras.module').then(m => m.ExtrasModule) },
      { path: 'pro', loadChildren: () => import('./pro/pro.module').then(m => m.ProModule) }
    ]
  },

  {
    path: 'iot',
    component: LayoutBasicComponent,
    children: [
      { path: 'tenant/tenantlist', component: TenantlistComponent },
      { path: 'user/userlist', component: UserlistComponent },
      { path: 'customer', loadChildren: () => import('./customer/customer.module').then(m => m.CustomerModule) },
      { path: 'device/devicelist', component: DevicelistComponent },
      { path: 'devicemodel/devicemodellist', component: DevicemodellistComponent },
      { path: 'device/devicegraph', component: DevicegraphComponent },
      { path: 'device/devicescene', component: DevicesceneComponent },
      { path: 'flow/designer', component: DesignerComponent },
      { path: 'flow/flowlist', component: FlowlistComponent },
      { path: 'flow/scriptlist', component: ScriptlistComponent },
      { path: 'flow/subscriptionlist', component: SubscriptionlistComponent },
      { path: 'flow/flowevents', component: FloweventsComponent },
      { path: 'flow/componentlist', component: ComponentlistComponent },
      { path: 'flow/taskexecutorlist', component: TaskexecutorlistComponent },
      { path: 'dictionary/dictionarylist', component: DictionarylistComponent },
      { path: 'dictionary/dictionarygrouplist', component: DictionarygrouplistComponent },
      { path: 'resouce/i18nlist', component: I18nlistComponent },
      { path: 'util/dynamicformlist', component: DynamicformlistComponent },
      { path: 'code/codeview', component: CodeviewComponent },
      { path: 'settings/certmgr', component: CertmgrComponent }
    ]
  },
  // Blak Layout 空白布局
  {
    path: 'data-v',
    component: LayoutBlankComponent,
    children: [{ path: '', loadChildren: () => import('./data-v/data-v.module').then(m => m.DataVModule) }]
  },
  // passport
  { path: '', loadChildren: () => import('./passport/passport.module').then(m => m.PassportModule), data: { preload: true } },
  { path: 'exception', loadChildren: () => import('./exception/exception.module').then(m => m.ExceptionModule) },
  { path: '**', redirectTo: 'exception/404' }
];

@NgModule({
  providers: [PreloadOptionalModules],
  imports: [
    RouterModule.forRoot(routes, {
      useHash: environment.useHash,
      // NOTICE: If you use `reuse-tab` component and turn on keepingScroll you can set to `disabled`
      // Pls refer to https://ng-alain.com/components/reuse-tab
      scrollPositionRestoration: 'top',
      preloadingStrategy: PreloadOptionalModules
    })
  ],
  exports: [RouterModule]
})
export class RouteRoutingModule {}
