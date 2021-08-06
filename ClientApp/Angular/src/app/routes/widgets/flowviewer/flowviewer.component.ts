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
  ViewChild,
} from '@angular/core';
import * as BpmnJS from 'bpmn-js/dist/bpmn-viewer.production.min.js';
import { interval } from 'rxjs';
@Component({
  selector: 'app-flowviewer',
  templateUrl: './flowviewer.component.html',
  styleUrls: ['./flowviewer.component.less'],
})
export class FlowviewerComponent implements AfterContentInit, OnChanges, OnDestroy {
  nodes = ['Event_1ugcz6g', 'Flow_1d2x8ry', 'Activity_1fl2v8i', 'Flow_09ib71g', 'Activity_1pl6xim', 'Flow_1nmme3b', 'Event_1e0aci8'];
  @ViewChild('viewer', { static: true })
  private viewer: ElementRef;
  @Input()
  diagramdata: any;
  private bpmnViewer: BpmnJS;

  constructor() {
    this.bpmnViewer = new BpmnJS({
      bpmnRenderer: {
        defaultFillColor: '#e6f7ff',
        defaultStrokeColor: '#1890ff',
      },
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
    console.log(this.diagramdata);
    await this.bpmnViewer.importXML(this.diagramdata.definitionsXml);

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
    var canvas = this.bpmnViewer.get('canvas');
    //只是修改了属性

    const souce = interval(1000).subscribe(async (x) => {
      var index = x % this.nodes.length;

      if (index == 0) {
        await this.bpmnViewer.importXML(this.diagramdata.definitionsXml);
      }
      if (index < this.nodes.length) {
        canvas.addMarker(this.nodes[index], 'flowviewhighlight');
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {}
  ngOnDestroy(): void {}
}
