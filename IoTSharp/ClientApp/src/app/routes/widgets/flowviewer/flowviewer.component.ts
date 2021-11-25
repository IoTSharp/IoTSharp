import {
  AfterContentInit,
  Component,
  ElementRef,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges,
  TemplateRef,
  ViewChild
} from '@angular/core';
import * as BpmnJS from 'bpmn-js/dist/bpmn-viewer.production.min.js';
import { interval } from 'rxjs';
@Component({
  selector: 'app-flowviewer',
  templateUrl: './flowviewer.component.html',
  styleUrls: ['./flowviewer.component.less']
})
export class FlowviewerComponent implements AfterContentInit, OnChanges, OnDestroy {
  @ViewChild('viewer', { static: true })
  private viewer!: ElementRef;
  @Input()
  diagramdata: any;
  private bpmnViewer: BpmnJS;

  constructor() {
    this.bpmnViewer = new BpmnJS({
      bpmnRenderer: {
        defaultFillColor: '#e6f7ff',
        defaultStrokeColor: '#1890ff'
      }
    });

    this.bpmnViewer.on('import.done', ({ error }) => {
      if (!error) {
        this.bpmnViewer.get('canvas').zoom('fit-viewport');
      }
    });
  }
  ngAfterContentInit(): void {
    this.bpmnViewer.attachTo(this.viewer.nativeElement);
  }
  async loadXml() {
    await this.bpmnViewer.importXML(this.diagramdata.definitionsXml);
    this.bpmnViewer.get('canvas').zoom('fit-viewport');
    var elementRegistry = this.bpmnViewer.get('elementRegistry');
    var shape = elementRegistry.get('Activity_1pl6xim');
    var overlays = this.bpmnViewer.get('overlays');
    //添加完附着物把返回的Id记住，不然附着物就没法操作了
    // var id = overlays.add('Activity_1pl6xim', {
    //   position: {
    //     top: 0,
    //     left: 0,
    //   },
    //   html:
    //     '<div class="highlight-overlay" style="width:' +
    //     shape.width +
    //     'px;height:' +
    //     shape.height +
    //     'px;background-color:red;display:block;opacity: 0.4">&nbsp;</div>',
    // });
    // console.log(overlays);
    // overlays.remove(id);

    //只是修改了属性
  }

  async redraw() {
    await this.bpmnViewer.importXML(this.diagramdata.definitionsXml);
  }
  sethighlight(bpmnid) {
    var canvas = this.bpmnViewer.get('canvas');
    //只能添加一次，再次添加只能重绘后添加了
    canvas.addMarker(bpmnid, 'flowviewhighlight');
  }

  ngOnChanges(changes: SimpleChanges): void {}
  ngOnDestroy(): void {}
}
