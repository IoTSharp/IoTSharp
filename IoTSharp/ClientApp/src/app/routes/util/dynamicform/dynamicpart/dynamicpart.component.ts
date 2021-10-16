import { ChangeDetectorRef, Component, ComponentFactoryResolver, Input, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { TextWidget } from '@delon/form';
import { _HttpClient } from '@delon/theme';
import { NzSelectComponent } from 'ng-zorro-antd/select';
import { controldirective } from '../controldirective';
import { TextBoxComponent } from '../cps/text-box/text-box.component';

@Component({
  selector: 'app-dynamicpart',
  templateUrl: './dynamicpart.component.html',
  styleUrls: ['./dynamicpart.component.less']
})
export class DynamicpartComponent implements OnInit {


  @Input()
  type: string = '0';
  @ViewChild(controldirective, { static: true })
  controlcontainer!: controldirective;
  viewContainerRef!: ViewContainerRef;
  constructor(private componentFactoryResolver: ComponentFactoryResolver, private http: _HttpClient, private cd: ChangeDetectorRef) { }

  ngOnInit(): void {
    switch (this.type) {
      case '1': {
        //这里使用cfr产生组件
        var component1 = this.componentFactoryResolver.resolveComponentFactory(TextBoxComponent);
        var viewContainerRef = this.controlcontainer.viewContainerRef;
        //组件的句柄，记得把引用暴露给上一层方便读取和设置属性，和angular/elements一样，textbox需要再包装(最好都包装一下，你不能放个光秃秃的控件上去哟)
        var handle = viewContainerRef.createComponent<TextBoxComponent>(component1, 0);
        break;
      }

     
      case '2': {
        var component2 = this.componentFactoryResolver.resolveComponentFactory(TextWidget);
        var viewContainerRef = this.controlcontainer.viewContainerRef;
        viewContainerRef.createComponent<TextWidget>(component2, 0);
        break;
      }
  
      case '3': {
        var component3 = this.componentFactoryResolver.resolveComponentFactory(NzSelectComponent);
        var viewContainerRef = this.controlcontainer.viewContainerRef;
        viewContainerRef.createComponent<NzSelectComponent>(component3, 0);
        break;
      }


    }
  }
}
