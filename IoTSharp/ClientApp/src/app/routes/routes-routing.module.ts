import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SimpleGuard } from '@delon/auth';
import { PreloadOptionalModules } from '@delon/theme';
import { environment } from '@env/environment';

// layout
import { LayoutBasicComponent } from '../layout/basic/basic.component';
import { LayoutBlankComponent } from '../layout/blank/blank.component';

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

      { path: 'extras', loadChildren: () => import('./extras/extras.module').then(m => m.ExtrasModule) },
      { path: 'pro', loadChildren: () => import('./pro/pro.module').then(m => m.ProModule) }
    ]
  },

  {
    path: 'iot',
    component: LayoutBasicComponent,
    children: [
      { path: 'assets', loadChildren: () => import('./assets/assets.module').then(m => m.AssetsModule) },
      { path: 'devices', loadChildren: () => import('./devices/devices.module').then(m => m.DevicesModule) },
      { path: 'produce', loadChildren: () => import('./produce/produce.module').then(m => m.ProduceModule) },
      { path: 'settings', loadChildren: () => import('./settings/settings.module').then(m => m.SettingsModule) },
      { path: 'alarms', loadChildren: () => import('./alarms/alarms.module').then(m => m.AlarmsModule) },
      { path: 'rules', loadChildren: () => import('./rules/rules.module').then(m => m.RulesModule) }
    ]
  },
  // Blak Layout 空白布局

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
