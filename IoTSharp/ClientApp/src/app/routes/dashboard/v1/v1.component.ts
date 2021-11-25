import { Platform } from '@angular/cdk/platform';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ComponentFactoryResolver, OnInit, ViewChild } from '@angular/core';
import { OnboardingService } from '@delon/abc/onboarding';
import { SettingsService, _HttpClient } from '@delon/theme';
import { WidgetItem } from '../../common/widgetItem';
import { HeaderkanbanComponent } from '../wedgits/headerkanban/headerkanban.component';
import { NewdeviceComponent } from '../wedgits/newdevice/newdevice.component';
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
    new WidgetItem('lists', NewdeviceComponent, {
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

    for (let item of this.widgets) {
      if (modules.find(x => x == item.name)) {
        const componentFactory = this.componentFactoryResolver.resolveComponentFactory(item.component);
        const viewContainerRef = this.widgetcontainer.viewContainerRef;
        const componentRef = viewContainerRef.createComponent<IWidgetComponent>(componentFactory);
        // componentRef.instance.someproperties={prop1:"value1"}
        //   componentRef.instance.dosomethingyouwanado()
      }
    }
  }
}
