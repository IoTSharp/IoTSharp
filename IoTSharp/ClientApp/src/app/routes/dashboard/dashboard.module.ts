import { NgModule } from '@angular/core';
import { CountDownModule } from '@delon/abc/count-down';
import { OnboardingModule } from '@delon/abc/onboarding';
import { QuickMenuModule } from '@delon/abc/quick-menu';
import { G2BarModule } from '@delon/chart/bar';
import { G2CardModule } from '@delon/chart/card';
import { G2GaugeModule } from '@delon/chart/gauge';
import { G2MiniAreaModule } from '@delon/chart/mini-area';
import { G2MiniBarModule } from '@delon/chart/mini-bar';
import { G2MiniProgressModule } from '@delon/chart/mini-progress';
import { NumberInfoModule } from '@delon/chart/number-info';
import { G2PieModule } from '@delon/chart/pie';
import { G2RadarModule } from '@delon/chart/radar';
import { G2SingleBarModule } from '@delon/chart/single-bar';
import { G2TagCloudModule } from '@delon/chart/tag-cloud';
import { G2TimelineModule } from '@delon/chart/timeline';
import { TrendModule } from '@delon/chart/trend';
import { G2WaterWaveModule } from '@delon/chart/water-wave';
import { SharedModule } from '@shared';
import { CountdownModule } from 'ngx-countdown';

import { DashboardAnalysisComponent } from './analysis/analysis.component';
import { DashboardRoutingModule } from './dashboard-routing.module';
import { DashboardMonitorComponent } from './monitor/monitor.component';
import { DashboardV1Component } from './v1/v1.component';
import { DashboardWorkplaceComponent } from './workplace/workplace.component';

import { HeaderkanbanComponent } from './wedgits/headerkanban/headerkanban.component';
import { WarningboardComponent } from './wedgits/warningboard/warningboard.component';
import { NewdeviceComponent } from './wedgits/newdevice/newdevice.component';
import { StatisticsComponent } from './wedgits/statistics/statistics.component';
import { AbmModule } from 'angular-baidu-maps';
import { widgetdirective } from './wedgits/widgetdirective';
const COMPONENTS = [DashboardV1Component, DashboardAnalysisComponent, DashboardMonitorComponent, DashboardWorkplaceComponent];
var Directives = [widgetdirective];
@NgModule({
  imports: [
    SharedModule,
    DashboardRoutingModule,
    CountDownModule,
    CountdownModule,
    G2BarModule,
    G2CardModule,
    G2GaugeModule,
    G2MiniAreaModule,
    G2MiniBarModule,
    G2MiniProgressModule,
    G2PieModule,
    G2RadarModule,
    G2SingleBarModule,
    G2TagCloudModule,
    G2TimelineModule,
    G2WaterWaveModule,
    NumberInfoModule,
    TrendModule,
    QuickMenuModule,
    OnboardingModule,
    AbmModule.forRoot({
      apiKey: 'Xgk3DdnaP9KNZdiKROD5Ad14BQGb3kYS' // app key为必选项
    })
  ],
  declarations: [...COMPONENTS, HeaderkanbanComponent, WarningboardComponent, NewdeviceComponent, StatisticsComponent, Directives]
})
export class DashboardModule {}
