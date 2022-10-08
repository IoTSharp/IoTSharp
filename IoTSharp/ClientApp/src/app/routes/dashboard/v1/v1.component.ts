import { Platform } from '@angular/cdk/platform';
import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ComponentFactoryResolver,
  Inject,
  OnInit,
  Renderer2,
  ViewChild
} from '@angular/core';
import type { Chart } from '@antv/g2';
import { OnboardingService } from '@delon/abc/onboarding';
import { SettingsService, _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { WidgetItem } from 'src/app/models/widgetItem';
import { HeaderkanbanComponent } from '../wedgits/headerkanban/headerkanban.component';

import { StatisticsComponent } from '../wedgits/statistics/statistics.component';
import { WarningboardComponent } from '../wedgits/warningboard/warningboard.component';
import { widgetdirective } from '../wedgits/widgetdirective';
import { IWidgetComponent } from './widgetcomponent';

@Component({
  selector: 'app-dashboard-v1',
  templateUrl: './v1.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardV1Component implements OnInit {
  @ViewChild(widgetdirective, { static: true })
  widgetcontainer!: widgetdirective;
  widgets = [
    new WidgetItem('kanban', HeaderkanbanComponent, {
      //  someneedtransferdata: "yourdata,don't forget declara a @Input someneedtransferdata Property ",
    }),
    new WidgetItem('statistics', StatisticsComponent, {
      //  someneedtransferdata: "yourdata,don't forget declara a @Input someneedtransferdata Property ",
    }),
    new WidgetItem('warning', WarningboardComponent, {
      //  someneedtransferdata: "yourdata,don't forget declara a @Input someneedtransferdata Property ",
    })
  ];
  constructor(
    private http: _HttpClient,
    private cdr: ChangeDetectorRef, // echarts有时候没出来，cdr刷新一下
    private settingService: SettingsService,
    private componentFactoryResolver: ComponentFactoryResolver
  ) {}

  ngOnInit(): void {
    this.initialwidgets();
  }

  initialwidgets() {
    var modules = this.settingService.user['modules'] ?? ['kanban', 'statistics', 'lists'];
    console.log(modules);
    for (let item of this.widgets) {
      if (modules.find(x => x == item.name)) {
        console.log(item.name);
        const componentFactory = this.componentFactoryResolver.resolveComponentFactory(item.component);
        const viewContainerRef = this.widgetcontainer.viewContainerRef;
        const componentRef = viewContainerRef.createComponent<IWidgetComponent>(componentFactory);
        // componentRef.instance.someproperties={prop1:"value1"}
        //   componentRef.instance.dosomethingyouwanado()
      }
    }
  }
}
