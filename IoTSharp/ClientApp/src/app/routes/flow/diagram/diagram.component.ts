import {
  AfterContentInit,
  Component,
  ElementRef,
  Input,
  OnChanges,
  OnDestroy,
  Output,
  ViewChild,
  SimpleChanges,
  EventEmitter,
  ChangeDetectorRef,
  Renderer2,
  Inject,
} from '@angular/core';
import { _HttpClient } from '@delon/theme'; //test
import { delay, mergeMap } from 'rxjs/operators';
import * as BpmnJS from 'bpmn-js/dist/bpmn-modeler.production.min.js';

import { Observable, from } from 'rxjs';
import { FormBuilder } from '@angular/forms';
import { appmessage } from '../../common/AppMessage';
import { NzConfigService } from 'ng-zorro-antd/core/config';
import { NzCodeEditorComponent } from 'ng-zorro-antd/code-editor';

import { DOCUMENT } from '@angular/common';
import { NzTooltipDirective } from 'ng-zorro-antd/tooltip';

@Component({
  selector: 'app-diagram',
  templateUrl: './diagram.component.html',
  styleUrls: ['./diagram.component.less'],
})

//gfx 属性消失代表元素被删除，不用去遍历整个SVG DOM判断，已知增加的钩子还有以下
// 'element.hover',
//'element.out',
// 'element.click',
// 'element.dblclick',
// 'element.mousedown',
// 'element.mouseup'
// 来自 https://github.com/bpmn-io/bpmn-js-examples/tree/master/interaction
export class DiagramComponent implements AfterContentInit, OnChanges, OnDestroy {
  nzEditorOption: { theme: 'vs'; language: 'json' };
  paramnzEditorOption: { theme: 'vs'; language: 'json' };
  @ViewChild(NzCodeEditorComponent, { static: false }) editorComponent?: NzCodeEditorComponent;
  isCollapsed = false;
  @ViewChild(NzTooltipDirective, { static: false }) tooltip?: NzTooltipDirective;
  executors = [];

  EMPTY_BPMN_DIAGRAM = `
  <?xml version="1.0" encoding="UTF-8"?>
  <definitions xmlns="http://www.omg.org/spec/BPMN/20100524/MODEL"
              xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI"
              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
              id="sid-38422fae-e03e-43a3-bef4-bd33b32041b2"
              targetNamespace="http://bpmn.io/bpmn"
              exporter="http://bpmn.io"
              exporterVersion="0.10.1">
   <process id="Process_1" isExecutable="false" />
   <bpmndi:BPMNDiagram id="BpmnDiagram_1">
     <bpmndi:BPMNPlane id="BpmnPlane_1" bpmnElement="Process_1" />
   </bpmndi:BPMNDiagram>
  </definitions>`;
  private bpmnJS: BpmnJS;

  nzHeader = '对象属性';

  @ViewChild('ref', { static: true })
  private el: ElementRef;
  @Output() private importDone: EventEmitter<any> = new EventEmitter();

  @Input() url: string;
  @Input() ruleId: number;
  loading = true;
  fullScreen = false;
  private document: Document;
  form: FormBpmnObject = {
    id: '',
    flowid: '',
    flowname: '',
    flowdesc: '',
    flowtype: '',
    nodeProcessClass: '',
    nodeProcessClassVisable: false,
    conditionexpression: '',
    conditionexpressionVisable: false,
    flowscript: '',
    flowscriptVisable: false,
    flowscripttype: '',
    flowscripttypeVisable: false,
    nodeProcessParams: '',
  };
  activity: Activity;
  selectedValue: any;
  panels = [
    {
      active: true,
      name: '对象属性',
      disabled: false
    },]

  toggleFullScreen(): void {
    this.fullScreen = !this.fullScreen;
    this.renderer.setStyle(this.document.body, 'overflow-y', this.fullScreen ? 'hidden' : null);
    this.editorComponent?.layout();
    this.tooltip?.hide();
  }
  flowscripttypeChange($event) {
    if (this.form.flowscripttype === 'executor') {
      this.form.nodeProcessClassVisable = true;
      this.form.flowscriptVisable = false;
    } else {
      this.form.nodeProcessClassVisable = false;
      this.form.flowscriptVisable = true;
    }

    this.nzConfigService.set('codeEditor', {
      defaultEditorOption: {
        language: this.form.flowscripttype,
        theme: 'vs',
      },
    });
    this.editorComponent?.layout();
  }

  ngModelChange($event) {
    var x = this.activity.sequenceFlows.find((x) => x.id == this.form.flowid);
    if (x != null) {
      x.bizObject = this.form;
      return;
    }

    var y = this.activity.tasks.find((x) => x.id == this.form.flowid);
    if (y) {
      y.bizObject = this.form;
      return;
    }

    var elementRegistry = this.bpmnJS.get('elementRegistry');

    var modeling = this.bpmnJS.get('modeling');
 
    // modeling.updateProperties(x.id, {
    //   name: 'ssss', //名称设置无效，如需双向绑定仍旧需要直接改Dom
    // });
  }
  public savediagram() {
    this.activity.ruleId = this.ruleId;

    from(this.bpmnJS.saveXML({ format: true }))
      .pipe(
        mergeMap((x: any) => {
          return this.http.post('api/rules/savediagram', {
            Xml: x.xml,
            Biz: JSON.stringify(this.activity),
          });
        }),
      )
      .subscribe();
  }

  getexcutors() {
    this.http.get('api/rules/getexecutors').subscribe(
      (next) => {
        this.executors = next.data;
      },
      (error) => {},
      () => {},
    );
  }

  constructor(
    private http: _HttpClient,
    private fb: FormBuilder,
    private cd: ChangeDetectorRef,
    private render: Renderer2,
    private nzConfigService: NzConfigService,
    private element: ElementRef,
    @Inject(DOCUMENT) document: any, private renderer: Renderer2
  ) {
    this.document = document;
    this.activity = new Activity();
    this.activity.tasks = [];
    this.activity.gateWays = [];
    this.activity.dataInputAssociations = [];
    this.activity.dataOutputAssociations = [];
    this.activity.sequenceFlows = [];
    this.activity.lane = [];
    this.activity.laneSet = [];
    this.activity.endEvents = [];
    this.activity.startEvents = [];
    this.activity.textAnnotations = [];
    this.bpmnJS = new BpmnJS({
      bpmnRenderer: {
        defaultFillColor: '#e6f7ff',
        defaultStrokeColor: '#1890ff',
      },
    });

    this.bpmnJS.on('import.done', ({ error }) => {
      if (!error) {
        this.bpmnJS.get('canvas').zoom('fit-viewport');
      }
    });

    this.bpmnJS.on('element.click', (event) => {
      //  this.form.patchValue({ Flowid: event.element.id, flowname: event.element.businessObject.name });
      this.hiddentools();
      switch (event.element.type) {
        case 'bpmn:Task':
          var task = this.activity.tasks.find((x) => x.id == event.element.id);
          if (task) {
            if (task.bizObject == null) {
              task.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                conditionexpressionVisable: false,
                flowscriptVisable: true,
                flowscripttypeVisable: true,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }
            task.bizObject.conditionexpressionVisable = false;
            task.bizObject.flowscripttypeVisable = true;
            if (task.bizObject.flowscripttype === 'executor') {
              task.bizObject.nodeProcessClassVisable = true;
              task.bizObject.flowscriptVisable = false;
            } else {
              task.bizObject.nodeProcessClassVisable = false;
              task.bizObject.flowscriptVisable = true;
            }

            this.form = task.bizObject;
          }

          break;
        case 'bpmn:EndEvent':
          var endevent = this.activity.endEvents.find((x) => x.id == event.element.id);
          if (endevent) {
            if (endevent.bizObject === null) {
              endevent.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            endevent.bizObject.nodeProcessClassVisable = false;
            this.form = endevent.bizObject;
          }

          break;
        case 'bpmn:StartEvent':
          var startevent = this.activity.startEvents.find((x) => x.id == event.element.id);
          if (startevent) {
            if (startevent.bizObject === null) {
              startevent.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            startevent.bizObject.nodeProcessClassVisable = false;
            this.form = startevent.bizObject;
          }

          break;
        case 'bpmn:IntermediateThrowEvent':
          var intermediatethrowevent = this.activity.endEvents.find((x) => x.id == event.element.id);
          if (intermediatethrowevent) {
            if (intermediatethrowevent.bizObject == null) {
              intermediatethrowevent.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            intermediatethrowevent.bizObject.nodeProcessClassVisable = false;
            this.form = intermediatethrowevent.bizObject;
          }

          break;
        case 'bpmn:ComplexGateway':
          var complexgateway = this.activity.gateWays.find((x) => x.id == event.element.id);
          if (complexgateway) {
            if (complexgateway.bizObject == null) {
              complexgateway.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            complexgateway.bizObject.nodeProcessClassVisable = true;
            this.form = complexgateway.bizObject;
          }

          break;
        case 'bpmn:ParallelGateway':
          var parallelgteway = this.activity.gateWays.find((x) => x.id == event.element.id);
          if (parallelgteway) {
            if (parallelgteway.bizObject == null) {
              parallelgteway.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            parallelgteway.bizObject.nodeProcessClassVisable = false;
            this.form = parallelgteway.bizObject;
          }

          break;
        case 'bpmn:ExclusiveGateway':
          var exclusivegateway = this.activity.gateWays.find((x) => x.id == event.element.id);
          if (exclusivegateway) {
            if (exclusivegateway.bizObject == null) {
              exclusivegateway.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            exclusivegateway.bizObject.nodeProcessClassVisable = true;
            this.form = exclusivegateway.bizObject;
          }

          break;
        case 'bpmn:InclusiveGateway':
          var inclusivegateway = this.activity.gateWays.find((x) => x.id == event.element.id);
          if (inclusivegateway) {
            if (inclusivegateway.bizObject == null) {
              inclusivegateway.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            inclusivegateway.bizObject.nodeProcessClassVisable = true;
            this.form = inclusivegateway.bizObject;
          }

          break;
        case 'bpmn:BusinessRuleTask':
          var businessruletask = this.activity.tasks.find((x) => x.id == event.element.id);
          if (businessruletask) {
            if (businessruletask.bizObject == null) {
              businessruletask.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: true,
                flowscripttypeVisable: true,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            businessruletask.bizObject.flowscriptVisable = true;
            businessruletask.bizObject.flowscripttypeVisable = true;
            businessruletask.bizObject.nodeProcessClassVisable = false;
            this.form = businessruletask.bizObject;
          }

          break;
        case 'bpmn:EventBasedGateway':
          var eventbasedgateway = this.activity.gateWays.find((x) => x.id == event.element.id);
          if (eventbasedgateway) {
            if (eventbasedgateway.bizObject == null) {
              eventbasedgateway.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            eventbasedgateway.bizObject.nodeProcessClassVisable = true;
            this.form = eventbasedgateway.bizObject;
          }

          break;
        case 'bpmn:ReceiveTask':
          var receivetask = this.activity.tasks.find((x) => x.id == event.element.id);
          if (receivetask) {
            if (receivetask.bizObject == null) {
              receivetask.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: true,
                flowscripttypeVisable: true,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            receivetask.bizObject.flowscriptVisable = true;
            receivetask.bizObject.flowscripttypeVisable = true;
            receivetask.bizObject.nodeProcessClassVisable = false;
            this.form = receivetask.bizObject;
          }

          break;
        case 'bpmn:UserTask':
          var usertask = this.activity.tasks.find((x) => x.id == event.element.id);
          if (usertask) {
            if (usertask.bizObject == null) {
              usertask.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: true,
                flowscripttypeVisable: true,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }
            this.form.flowscript = '';
            this.form.flowscripttype = '';
            usertask.bizObject.flowscriptVisable = true;
            usertask.bizObject.flowscripttypeVisable = true;
            usertask.bizObject.nodeProcessClassVisable = false;
            this.form = usertask.bizObject;
          }

          break;
        case 'bpmn:IntermediateCatchEvent':
          var intermediatecatchevent = this.activity.endEvents.find((x) => x.id == event.element.id);
          if (intermediatecatchevent) {
            if (intermediatecatchevent.bizObject == null) {
              intermediatecatchevent.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            intermediatecatchevent.bizObject.nodeProcessClassVisable = false;
            this.form = intermediatecatchevent.bizObject;
          }

          break;
        case 'bpmn:ServiceTask':
          var servicetask = this.activity.tasks.find((x) => x.id == event.element.id);
          if (servicetask) {
            if (servicetask.bizObject == null) {
              servicetask.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: true,
                flowscripttypeVisable: true,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            servicetask.bizObject.flowscriptVisable = true;
            servicetask.bizObject.flowscripttypeVisable = true;
            servicetask.bizObject.nodeProcessClassVisable = false;
            this.form = servicetask.bizObject;
          }

          break;
        case 'bpmn:ManualTask':
          var manualtask = this.activity.tasks.find((x) => x.id == event.element.id);
          if (manualtask) {
            if (manualtask.bizObject == null) {
              manualtask.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: true,
                flowscripttypeVisable: true,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            manualtask.bizObject.flowscriptVisable = true;
            manualtask.bizObject.flowscripttypeVisable = true;
            manualtask.bizObject.nodeProcessClassVisable = false;
            this.form = manualtask.bizObject;
          }

          break;
        case 'bpmn:SendTask':
          var sendtask = this.activity.tasks.find((x) => x.id == event.element.id);
          if (sendtask) {
            if (sendtask.bizObject == null) {
              sendtask.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: true,
                flowscripttypeVisable: true,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            sendtask.bizObject.flowscriptVisable = true;
            sendtask.bizObject.flowscripttypeVisable = true;
            sendtask.bizObject.nodeProcessClassVisable = false;
            this.form = sendtask.bizObject;
          }

          break;
        case 'bpmn:CallActivity':
          var callactivity = this.activity.tasks.find((x) => x.id == event.element.id);
          if (callactivity) {
            if (callactivity.bizObject == null) {
              callactivity.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: true,
                flowscripttypeVisable: true,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }
            callactivity.bizObject.flowscriptVisable = true;
            callactivity.bizObject.flowscripttypeVisable = true;
            callactivity.bizObject.nodeProcessClassVisable = false;
            this.form = callactivity.bizObject;
          }

          break;
        case 'bpmn:SequenceFlow':
          var sequenceflow = this.activity.sequenceFlows.find((x) => x.id == event.element.id);
          if (sequenceflow) {
            if (!sequenceflow.bizObject) {
              sequenceflow.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            sequenceflow.bizObject.conditionexpressionVisable = true;
            sequenceflow.bizObject.nodeProcessClassVisable = false;
            this.form = sequenceflow.bizObject;
          }

          break;
        case 'bpmn:Participant':
          var participant = this.activity.containers.find((x) => x.id == event.element.id);
          if (participant) {
            if (participant.bizObject == null) {
              participant.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }
            participant.bizObject.nodeProcessClassVisable = false;
            this.form = participant.bizObject;
          }

          break;
        case 'bpmn:SubProcess':
          var subprocess = this.activity.tasks.find((x) => x.id == event.element.id);
          if (subprocess) {
            if (subprocess.bizObject == null) {
              subprocess.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }

            subprocess.bizObject.nodeProcessClassVisable = false;
            this.form = subprocess.bizObject;
          }

          break;
        case 'bpmn:Collaboration':
          var collaboration = this.activity.containers.find((x) => x.id == event.element.id);
          if (collaboration) {
            if (collaboration.bizObject == null) {
              collaboration.bizObject = {
                id: '',
                flowid: '',
                flowname: '',
                flowdesc: '',
                flowtype: '',
                flowscript: '',
                conditionexpressionVisable: false,
                nodeProcessClass: '',
                conditionexpression: '',
                nodeProcessClassVisable: true,
                flowscriptVisable: false,
                flowscripttypeVisable: false,
                flowscripttype: '',
                nodeProcessParams: '',
              };
            }
            collaboration.bizObject.nodeProcessClassVisable = false;
            this.form = collaboration.bizObject;
          }
          break;
      }
      this.cd.detectChanges();
    });
    this.bpmnJS.on('element.changed', (event) => {
      if (event.element.type.indexOf('bpmn') !== -1) {
      }

      switch (event.element.type) {
        case 'bpmn:Task':
          this.doTask(event); 
          break;
        case 'bpmn:EndEvent':
          this.doEndEvent(event);
          break;
        case 'bpmn:StartEvent':
          this.doStartEvent(event);
          break;
        case 'bpmn:IntermediateThrowEvent':
          this.doGateWay(event);
          break;
        case 'bpmn:ComplexGateway':
          this.doGateWay(event);
          break;
        case 'bpmn:ParallelGateway':
          this.doGateWay(event);
          break;
        case 'bpmn:ExclusiveGateway':
          this.doGateWay(event);
          break;
        case 'bpmn:InclusiveGateway':
          this.doGateWay(event);
          break;
        case 'bpmn:EventBasedGateway':
          this.doGateWay(event);
          break;
        case 'bpmn:BusinessRuleTask':
          this.doTask(event);
          break;
        case 'bpmn:ReceiveTask':
          this.doTask(event);
          break;
        case 'bpmn:UserTask':
          this.doTask(event);
          break;
        case 'bpmn:IntermediateCatchEvent':
          this.doGateWay(event);
          break;
        case 'bpmn:ServiceTask':
          this.doTask(event);     
          break;
        case 'bpmn:ManualTask':
          this.doTask(event);
          break;
        case 'bpmn:SendTask':
          this.doTask(event);
          break;
        case 'bpmn:CallActivity':
          this.doTask(event);
          break;

        case 'bpmn:Lane':
          this.DoContainer(event);
          break;
        case 'bpmn:Participant':
          this.DoContainer(event);
          break;
        case 'bpmn:DataStoreReference':
          this.DoDataStoreReference(event);
          break;
        case 'bpmn:SubProcess':
          this.DoSubProcess(event);
          break;
        case 'bpmn:SequenceFlow':
          this.doSequenceFlow(event);
          break;

        case 'bpmn:TextAnnotation':
          this.doTextAnnotation(event);
          break;
        default:
          this.DoBaseBpmnObject(event);
          break;
      }
      setTimeout(()=>{
        this.hiddentools();
      },100)


      
  

    });
 
    this.getexcutors();     
  }
  ngAfterContentInit(): void {
    this.bpmnJS.attachTo(this.el.nativeElement);
    this.render.setStyle(this.el.nativeElement, 'height', window.innerHeight - 64 + 'px');

    this.hiddentools();
  }

  // hide some elements
  private hiddentools() {
    this.el.nativeElement.querySelectorAll('.group').forEach((element) => {
      if (element.getAttribute('data-group') == 'data-store') {
        element.style.display = 'none';
      }

      if (element.getAttribute('data-group') == 'data-store') {
        element.style.display = 'none';
      }

      if (element.getAttribute('data-group') == 'collaboration') {
        element.style.display = 'none';
      }

      if (element.getAttribute('data-group') == 'data-object') {
        element.style.display = 'none';
      }
      if (element.getAttribute('data-group') == 'gateway') {
        element.style.display = 'none';
      }
      if (element.getAttribute('data-group') == 'artifact') {
        element.style.display = 'none';
      }

      if (element.getAttribute('data-group') == 'edit') {
        element.querySelectorAll('.bpmn-icon-screw-wrench').forEach((e) => {
          if (e.getAttribute('data-action') == 'replace') {
            e.style.display = 'none';

          }
        });
      }
    });
    this.el.nativeElement.querySelectorAll('.bpmn-icon-gateway-none').forEach((element) => {
      element.style.display = 'none';
    });
    this.el.nativeElement.querySelectorAll('.bpmn-icon-subprocess-expanded').forEach((element) => {
      element.style.display = 'none';
    });
    this.el.nativeElement.querySelectorAll('.bpmn-icon-intermediate-event-none').forEach((element) => {
      element.style.display = 'none';
    });
    this.el.nativeElement.querySelectorAll('.bpmn-icon-lasso-tool').forEach((element) => {
      element.style.display = 'none';
    });

    this.el.nativeElement.querySelectorAll('.bpmn-icon-text-annotation').forEach((element) => {
      element.style.display = 'none';
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.url) {
      this.loadUrl(changes.url.currentValue);
    }
  }

  doTextAnnotation(e: any) {
    if (e.gfx) {
      var baseBpmnObject = this.activity.textAnnotations.find((x) => x.id === e.element.businessObject.id);

      if (baseBpmnObject) {
        baseBpmnObject.id = e.element.id;
        baseBpmnObject.bpmntype = e.element.type;
        baseBpmnObject.bizObject.flowname = e.element.businessObject.name;
        baseBpmnObject.bizObject.flowid = e.element.id;
      } else {
        baseBpmnObject = new TextAnnotation();
        baseBpmnObject.id = e.element.businessObject.id;
        baseBpmnObject.text = e.element.businessObject.text;
        baseBpmnObject.bpmntype = e.element.type;
        baseBpmnObject.bizObject = {
          id: '',
          flowid: '',
          flowname: '',
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: true,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        baseBpmnObject.outgoing = [];
        baseBpmnObject.incoming = [];
        baseBpmnObject.bizObject.flowname = e.element.businessObject.name;
        baseBpmnObject.bizObject.flowid = e.element.id;
        this.activity.textAnnotations = [...this.activity.textAnnotations, baseBpmnObject];
      }
      //TextAnnotation只有incoming
      baseBpmnObject.incoming = [
        ...baseBpmnObject.incoming,
        ...e.element.incoming.map((x) => {
          return { id: x.id, name: x.name, sourceId: e.element.businessObject.targetRef.id };
        }),
      ];
    } else {
      this.activity.textAnnotations = this.activity.textAnnotations.filter((x) => x.id != e.element.id) ?? [];
    }
  }

  DoBaseBpmnObject(e: any): void {
    if (e.gfx) {
      var baseBpmnObject = this.activity.baseBpmnObjects.find((x) => x.id === e.element.businessObject.id);

      if (baseBpmnObject) {
        baseBpmnObject.id = e.element.id;
        baseBpmnObject.bpmntype = e.element.type;
        baseBpmnObject.bizObject.flowname = e.element.businessObject.name;
        baseBpmnObject.bizObject.flowid = e.element.id;
      } else {
        baseBpmnObject = new SequenceFlow();
        baseBpmnObject.id = e.element.businessObject.id;
        baseBpmnObject.bpmntype = e.element.type;
        baseBpmnObject.bizObject = {
          id: '',
          flowid: '',
          flowname: '',
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: true,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        baseBpmnObject.outgoing = [];
        baseBpmnObject.incoming = [];
        baseBpmnObject.bizObject.flowname = e.element.businessObject.name;
        baseBpmnObject.bizObject.flowid = e.element.id;
        this.activity.baseBpmnObjects = [...this.activity.baseBpmnObjects, baseBpmnObject];
      }
      console.log('baseBpmnObject');
      baseBpmnObject.incoming = [
        ...baseBpmnObject.incoming,
        ...e.element.incoming.map((x) => {
          return { id: x.id, name: x.name, sourceId: e.element.businessObject.targetRef.id };
        }),
      ];

      baseBpmnObject.outgoing = [
        ...baseBpmnObject.outgoing,
        ...e.element.outgoing.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
    } else {
      this.activity.baseBpmnObjects = this.activity.baseBpmnObjects.filter((x) => x.id != e.element.id) ?? [];
    }
  }
  DoDataStoreReference(e: any): void {
    if (e.gfx) {
      var dataStoreReference = this.activity.dataStoreReferences.find((x) => x.id === e.element.businessObject.id);

      if (dataStoreReference) {
        dataStoreReference.id = e.element.id;
        dataStoreReference.bizObject.flowname = e.element.businessObject.name;
        dataStoreReference.bizObject.flowid = e.element.id;
        dataStoreReference.bpmntype = e.element.type;
      } else {
        dataStoreReference = new SequenceFlow();
        dataStoreReference.id = e.element.businessObject.id;
        dataStoreReference.bpmntype = e.element.type;
        dataStoreReference.bizObject = {
          id: '',
          flowid: '',
          flowname: '',
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: true,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        dataStoreReference.outgoing = [];
        dataStoreReference.incoming = [];
        dataStoreReference.bizObject.flowname = e.element.businessObject.name;
        dataStoreReference.bizObject.flowid = e.element.id;
        this.activity.dataStoreReferences = [...this.activity.dataStoreReferences, dataStoreReference];
      }
      dataStoreReference.incoming = [
        ...dataStoreReference.incoming,
        ...e.element.incoming.map((x) => {
          return { id: x.id, name: x.name, sourceId: e.element.businessObject.targetRef.id };
        }),
      ];
      dataStoreReference.outgoing = [
        ...dataStoreReference.outgoing,
        ...e.element.outgoing.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
    } else {
      this.activity.dataStoreReferences = this.activity.dataStoreReferences.filter((x) => x.id != e.element.id) ?? [];
    }
  }
  DoContainer(e: any): void {
    if (e.gfx) {
      var container = this.activity.containers.find((x) => x.id === e.element.businessObject.id);

      if (container) {
        container.id = e.element.id;
        container.bizObject.flowname = e.element.businessObject.name;
        container.bizObject.flowid = e.element.id;
        container.bpmntype = e.element.type;
      } else {
        container = new SequenceFlow();
        container.id = e.element.businessObject.id;
        container.bpmntype = e.element.type;
        container.bizObject = {
          id: '',
          flowid: '',
          flowname: '',
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: true,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        container.outgoing = [];
        container.incoming = [];
        container.bizObject.flowname = e.element.businessObject.name;
        container.bizObject.flowid = e.element.id;
        this.activity.containers = [...this.activity.containers, container];
      }

      container.incoming = [
        ...container.incoming,
        ...e.element.incoming.map((x) => {
          return { id: x.id, name: x.name, sourceId: e.element.businessObject.targetRef.id };
        }),
      ];

      container.outgoing = [
        ...container.outgoing,
        ...e.element.outgoing.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
    } else {
      this.activity.containers = this.activity.containers.filter((x) => x.id != e.element.id) ?? [];
    }
  }
  DoSubProcess(e: any): void {
    if (e.gfx) {
      let subProcess = this.activity.subProcesses.find((x) => x.id === e.element.businessObject.id);

      if (subProcess) {
        subProcess.id = e.element.id;
        subProcess.bpmntype = e.element.type;
        subProcess.bizObject.flowname = e.element.businessObject.name;
        subProcess.bizObject.flowid = e.element.id;
      } else {
        subProcess = new SequenceFlow();
        subProcess.id = e.element.businessObject.id;
        subProcess.bpmntype = e.element.type;
        subProcess.bizObject = {
          id: '',
          flowid: '',
          flowname: '',
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: true,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        subProcess.outgoing = [];
        subProcess.incoming = [];
        subProcess.bizObject.flowname = e.element.businessObject.name;
        subProcess.bizObject.flowid = e.element.id;
        this.activity.subProcesses = [...this.activity.subProcesses, subProcess];
      }

      subProcess.incoming = [
        ...subProcess.incoming,
        ...e.element.incoming.map((x) => {
          return { id: x.id, name: x.name, sourceId: e.element.businessObject.targetRef.id };
        }),
      ];

      subProcess.outgoing = [
        ...subProcess.outgoing,
        ...e.element.outgoing.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
    } else {
      this.activity.subProcesses = this.activity.subProcesses.filter((x) => x.id != e.element.id) ?? [];
    }
  }

  doSequenceFlow(e: any): void {

    if (e.gfx) {
      let sequenceflow = this.activity.sequenceFlows.find((x) => x.id === e.element.businessObject.id);

      if (sequenceflow) {
        sequenceflow.id = e.element.id;
        sequenceflow.bpmntype = e.element.type;
        sequenceflow.bizObject.flowname = e.element.businessObject.name;
        sequenceflow.bizObject.flowid = e.element.id;
      } else {
        sequenceflow = new SequenceFlow();
        sequenceflow.id = e.element.businessObject.id;
        sequenceflow.bpmntype = e.element.type;
        sequenceflow.bizObject = {
          id: '',
          flowid: '',
          flowname: '',
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: true,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        sequenceflow.outgoing = [];
        sequenceflow.incoming = [];
        sequenceflow.sourceId = e.element.businessObject.sourceRef.id;
        sequenceflow.targetId = e.element.businessObject.targetRef.id;
        sequenceflow.bizObject.flowname = e.element.businessObject.name;
        sequenceflow.bizObject.flowid = e.element.id;
        this.activity.sequenceFlows = [...this.activity.sequenceFlows, sequenceflow];
      }

      sequenceflow.incoming = [
        ...sequenceflow.incoming,
        ...e.element.incoming.map((x) => {
          return { id: x.id, name: x.name, sourceId: e.element.businessObject.targetRef.id };
        }),
      ];

      sequenceflow.outgoing = [
        ...sequenceflow.outgoing,
        ...e.element.outgoing.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
    } else {
      this.activity.sequenceFlows = this.activity.sequenceFlows.filter((x) => x.id != e.element.id) ?? [];
    }


  }

  doTask(e: any): void {
    if (e.gfx) {
      var task = this.activity.tasks.find((x) => x.id === e.element.businessObject.id);

      if (task) {
        task.id = e.element.id;
        task.bpmntype = e.element.type;
        task.bizObject.flowname = e.element.businessObject.name;
        task.bizObject.flowid = e.element.id;
      } else {
        task = new Task();
        task.id = e.element.id;
        task.bpmntype = e.element.type;
        task.bizObject = {
          id: '',
          flowid: '',
          flowname: '',
          flowdesc: '',
          flowtype: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: true,
          flowscript: '',
          flowscriptVisable: true,
          flowscripttypeVisable: true,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        task.id = e.element.businessObject.id;
        task.bizObject.flowname = e.element.businessObject.name;
        task.bizObject.flowid = e.element.id;
        this.activity.tasks = [...this.activity.tasks, task];
      }
      task.incoming = [];
      task.incoming = [
        ...task.incoming,
        ...e.element.incoming.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
      task.outgoing = [];
      task.outgoing = [
        ...task.outgoing,
        ...e.element.outgoing.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
    } else {
      this.activity.tasks = this.activity.tasks.filter((x) => x.id != e.element.id) ?? [];
    }
  }

  doGateWay(e: any): void {
    if (e.gfx) {
      var gateway = this.activity.gateWays.find((x) => x.id === e.element.businessObject.id);

      if (gateway) {
        gateway.id = e.element.id;

        gateway.bpmntype = e.element.type;
        gateway.bizObject.flowname = e.element.businessObject.name;
      } else {
        gateway = new GateWay();
        gateway.id = e.element.id;
        gateway.bpmntype = e.element.type;
        gateway.bizObject = {
          id: '',
          flowid: '',
          flowname: '',
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: true,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        gateway.id = e.element.businessObject.id;
        gateway.outgoing = [];
        gateway.incoming = [];
        gateway.bizObject.flowname = e.element.businessObject.name;
        gateway.bizObject.flowid = e.element.id;
        this.activity.gateWays = [...this.activity.gateWays, gateway];
      }
      gateway.incoming = [
        ...gateway.incoming,
        ...e.element.incoming.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
      gateway.outgoing = [
        ...gateway.outgoing,
        ...e.element.outgoing.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
    } else {
      this.activity.gateWays = this.activity.gateWays.filter((x) => x.id != e.element.id) ?? [];
    }
  }

  doStartEvent(e: any): void {
    var startevent = this.activity.startEvents.find((x) => x.id === e.element.businessObject.id);
    if (e.gfx) {
      if (startevent) {
        startevent.bpmntype = e.element.type;
        startevent.id = e.element.id;
        startevent.bizObject.flowname = e.element.businessObject.name;
      } else {
        startevent = new BpmnBaseObject();
        startevent.bpmntype = e.element.type;
        startevent.id = e.element.id;
        startevent.bizObject = {
          id: '',
          flowid: '',
          flowname: '',
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          nodeProcessClassVisable: true,
          conditionexpression: '',
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        startevent.id = e.element.businessObject.id;
        startevent.outgoing = [];
        startevent.incoming = [];
        startevent.bizObject.flowname = e.element.businessObject.name;
        startevent.bizObject.flowid = e.element.id;
        this.activity.startEvents = [...this.activity.startEvents, startevent];
      }
      startevent.incoming = [
        ...startevent.incoming,
        ...e.element.incoming.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
      startevent.outgoing = [
        ...startevent.outgoing,
        ...e.element.outgoing.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
    } else {
      this.activity.startEvents = this.activity.startEvents.filter((x) => x.id != e.element.id) ?? [];
    }
  }
  doEndEvent(e: any): void {
    var endevent = this.activity.endEvents.find((x) => x.id === e.element.businessObject.id);
    if (e.gfx) {
      if (endevent) {
        endevent.id = e.element.id;
        endevent.bpmntype = e.element.type;
        endevent.bizObject.flowname = e.element.businessObject.name;
      } else {
        endevent = new BpmnBaseObject();
        endevent.id = e.element.id;
        endevent.bpmntype = e.element.type;
        endevent.bizObject = {
          id: '',
          flowid: '',
          flowname: '',
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: true,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        endevent.id = e.element.businessObject.id;
        endevent.outgoing = [];
        endevent.incoming = [];
        endevent.bizObject.flowname = e.element.businessObject.name;
        endevent.bizObject.flowid = e.element.id;
        this.activity.endEvents = [...this.activity.endEvents, endevent];
      }
      endevent.incoming = [
        ...endevent.incoming,
        ...e.element.incoming.map((x: { id: any; name: any }) => {
          return { id: x.id, name: x.name };
        }),
      ];
      endevent.outgoing = [
        ...endevent.outgoing,
        ...e.element.outgoing.map((x) => {
          return { id: x.id, name: x.name };
        }),
      ];
    } else {
      this.activity.endEvents = this.activity.endEvents.filter((x) => x.id != e.element.id) ?? [];
    }
  }

  ngOnDestroy(): void {
    this.bpmnJS.destroy();
  }
  randomString(e): string {
    e = e || 32;
    var t = 'ABCDEFGHJKMNPQRSTWXYZabcdefhijkmnprstwxyz2345678',
      a = t.length,
      n = '';
    for (let i = 0; i < e; i++) n += t.charAt(Math.floor(Math.random() * a));
    return n;
  }

  loadUrl(url: string) {
    if (url === '') {
      this.bpmnJS.importXML(this.EMPTY_BPMN_DIAGRAM);
      this.bpmnJS.get('canvas').zoom('fit-viewport');
      this.activity = new Activity();
      this.activity.sequenceFlows = [];
      this.activity.tasks = [];
      this.activity.gateWays = [];
      this.activity.lane = [];
      this.activity.laneSet = [];
      this.activity.endEvents = [];
      this.activity.startEvents = [];
      this.activity.baseBpmnObjects = [];
      this.activity.dataStoreReferences = [];
      this.activity.subProcesses = [];
      this.activity.dataOutputAssociations = [];
      this.activity.dataInputAssociations = [];
      this.activity.ruleId = 0;
    } else {
      this.http.get<appmessage<DesignerResult>>(url).subscribe(
        async (data) => {
          this.activity = new Activity();
          this.activity.sequenceFlows = [];
          this.activity.tasks = [];
          this.activity.gateWays = [];
          this.activity.lane = [];
          this.activity.laneSet = [];
          this.activity.endEvents = [];
          this.activity.startEvents = [];
          this.activity.baseBpmnObjects = [];
          this.activity.dataStoreReferences = [];
          this.activity.subProcesses = [];
          this.activity.dataOutputAssociations = [];
          this.activity.dataInputAssociations = [];
          this.activity.ruleId = 0;

          await this.bpmnJS.importXML(data.data.xml);
          this.InitData(data.data);
          this.bpmnJS.get('canvas').zoom('fit-viewport');

          //  before bpmn 7.x
          // this.bpmnJS.importXML(data.Xml,
          //   (err: any, warnings: string | undefined) => {

          //     if (err) {

          //     } else {

          //     }
          //   });
          // this.importDone.emit({
          //   type: 'success',
          //   data
          // });
        },
        (err) => {
          this.importDone.emit({
            type: 'error',
            error: err,
          });
        },
        () => {
          //finally
        },
      );
    }
  }

  private InitData(data: any): void {
    if (data.gateWays) {
      for (var element of data.gateWays) {
        var gateWay = new GateWay();
        gateWay.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,

          nodeProcessClass: element.bizObject.NodeProcessClass,
          conditionexpression: '',
          nodeProcessClassVisable: true,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        gateWay.incoming = element.incoming ?? [];
        gateWay.outgoing == element.incoming ?? [];
        gateWay.id = element.id;
        gateWay.bpmntype = element.bpmntype;
        this.activity.gateWays.push(gateWay);
      }
    }
    if (data.sequenceFlows) {
      for (var element of data.sequenceFlows) {
        var sequenceflows = new SequenceFlow();
        sequenceflows.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.flowname,

          flowdesc: '',
          flowtype: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          flowscript: '',
          conditionexpression: element.bizObject.conditionexpression,
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        sequenceflows.sourceId = element.sourceId;
        sequenceflows.targetId = element.targetId;
        sequenceflows.incoming = element.incoming ?? [];
        sequenceflows.outgoing = element.incoming ?? [];
        sequenceflows.id = element.id;
        sequenceflows.bpmntype = element.bpmntype;

        this.activity.sequenceFlows.push(sequenceflows);
      }
    }
    if (data.tasks) {
      for (var element of data.tasks) {
        var task = new Task();
        task.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: element.bizObject.flowscript ?? '',
          conditionexpressionVisable: false,
          nodeProcessClass: element.bizObject.nodeProcessClass,
          nodeProcessParams: element.bizObject.nodeProcessParams,
          conditionexpression: '',
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: element.bizObject.flowscripttype ?? '',
        };

        task.incoming = element.incoming ?? [];
        task.outgoing = element.incoming ?? [];
        task.id = element.id;
        task.bpmntype = element.bpmntype;
        this.activity.tasks.push(task);
      }
    }

    if (data.laneSet) {
      for (var element of data.laneSet) {
        var laneset = new BpmnBaseObject();
        laneset.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        laneset.incoming = element.incoming ?? [];
        laneset.outgoing = element.incoming ?? [];
        laneset.id = element.id;
        laneset.bpmntype = element.bpmntype;
        this.activity.laneSet.push(laneset);
      }
    }
    if (data.endEvents) {
      for (var element of data.endEvents) {
        var endevent = new BpmnBaseObject();
        endevent.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        endevent.incoming = element.incoming ?? [];
        endevent.outgoing = element.incoming ?? [];
        endevent.id = element.id;
        endevent.bpmntype = element.bpmntype;
        this.activity.endEvents.push(endevent);
      }
    }

    if (data.startEvents) {
      for (var element of data.startEvents) {
        var startevent = new BpmnBaseObject();
        startevent.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        startevent.incoming = [];
        startevent.outgoing = [];
        startevent.id = element.id;
        startevent.bpmntype = element.bpmntype;
        this.activity.startEvents.push(startevent);
      }
    }

    if (data.containers) {
      for (var element of data.containers) {
        var container = new BpmnBaseObject();
        container.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        container.incoming = element.incoming ?? [];
        container.outgoing = element.incoming ?? [];
        container.id = element.id;
        container.bpmntype = element.bpmntype;
        this.activity.containers.push(container);
      }
    }

    if (data.baseBpmnObjects) {
      for (var element of data.baseBpmnObjects) {
        var baseBpmnObject = new BpmnBaseObject();
        baseBpmnObject.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        baseBpmnObject.incoming = element.incoming ?? [];
        baseBpmnObject.outgoing = element.incoming ?? [];
        baseBpmnObject.id = element.id;
        baseBpmnObject.bpmntype = element.bpmntype;
        this.activity.baseBpmnObjects.push(baseBpmnObject);
      }
    }

    if (data.dataStoreReferences) {
      for (var element of data.dataStoreReferences) {
        var datastorereference = new BpmnBaseObject();
        datastorereference.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        datastorereference.incoming = element.incoming ?? [];
        datastorereference.outgoing = element.incoming ?? [];
        datastorereference.id = element.id;
        datastorereference.bpmntype = element.bpmntype;
        this.activity.dataStoreReferences.push(datastorereference);
      }
    }
    if (data.subProcesses) {
      for (var element of data.subProcesses) {
        var subprocess = new BpmnBaseObject();
        subprocess.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        subprocess.incoming = element.incoming ?? [];
        subprocess.outgoing = element.incoming ?? [];
        subprocess.id = element.id;
        subprocess.bpmntype = element.bpmntype;
        this.activity.subProcesses.push(subprocess);
      }
    }

    if (data.dataOutputAssociations) {
      for (var element of data.dataOutputAssociations) {
        var dataOutputAssociation = new DataOutputAssociation();
        dataOutputAssociation.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        dataOutputAssociation.incoming = element.incoming ?? [];
        dataOutputAssociation.outgoing = element.incoming ?? [];
        dataOutputAssociation.id = element.id;
        dataOutputAssociation.bpmntype = element.bpmntype;
        this.activity.dataOutputAssociations.push(dataOutputAssociation);
      }
    }

    if (data.dataInputAssociations) {
      for (var element of data.dataInputAssociations) {
        var dataInputAssociations = new DataOutputAssociation();
        dataInputAssociations.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        dataInputAssociations.incoming = element.incoming ?? [];
        dataInputAssociations.outgoing = element.incoming ?? [];
        dataInputAssociations.id = element.id;
        dataInputAssociations.bpmntype = element.bpmntype;
        this.activity.dataInputAssociations.push(dataInputAssociations);
      }
    }

    if (data.lane) {
      for (var element of data.lane) {
        var lane = new BpmnBaseObject();
        lane.bizObject = {
          id: element.bizObject.flowid,
          flowid: element.id,
          flowname: element.bizObject.flowname,
          flowdesc: '',
          flowtype: '',
          flowscript: '',
          conditionexpressionVisable: false,
          nodeProcessClass: '',
          conditionexpression: '',
          nodeProcessClassVisable: false,
          flowscriptVisable: false,
          flowscripttypeVisable: false,
          flowscripttype: '',
          nodeProcessParams: '',
        };
        lane.incoming = element.incoming ?? [];
        lane.outgoing = element.incoming ?? [];
        lane.id = element.id;
        lane.bpmntype = element.bpmntype;
        this.activity.lane.push(lane);
      }
    }
  }
}

export class BasebizObject {
  flowid!: String;
  flowname!: String;
}

export class DesignerResult {
  public biz!: Activity;
  public xml!: String;
}
export class BpmnBaseObject {
  public id!: String;
  public bpmntype!: String;
  public incoming!: BpmnBaseObject[];
  public outgoing!: BpmnBaseObject[];
  public bizObject: FormBpmnObject = {
    id: '',
    flowid: '',
    flowname: '',
    flowdesc: '',
    flowtype: '',
    flowscript: '',
    conditionexpressionVisable: false,
    nodeProcessClass: '',
    nodeProcessClassVisable: true,
    conditionexpression: '',
    flowscriptVisable: false,
    flowscripttypeVisable: false,
    flowscripttype: '',
    nodeProcessParams: '',
  };
}

export class Activity {
  public sequenceFlows!: SequenceFlow[];
  public tasks!: Task[];
  public gateWays!: GateWay[];
  public lane!: BpmnBaseObject[];
  public laneSet!: BpmnBaseObject[];
  public endEvents!: BpmnBaseObject[];
  public startEvents!: BpmnBaseObject[];
  public textAnnotations!: TextAnnotation[];

  public containers!: BpmnBaseObject[];
  public baseBpmnObjects!: BpmnBaseObject[];
  public dataStoreReferences!: BpmnBaseObject[];
  public subProcesses!: BpmnBaseObject[];
  public dataOutputAssociations!: DataOutputAssociation[];
  public dataInputAssociations!: DataOutputAssociation[];

  public ruleId!: Number;

  public definitionsStatus!: Number;
}
export class TextAnnotation extends BpmnBaseObject {
  public text!: String;
}

export class Task extends BpmnBaseObject {
  public flowtype!: String;
  public flowId!: Number;
}
export class GateWay extends BpmnBaseObject {
  public nodeProcessClass!: String;
  public sourceId!: String;
  public targetId!: String;
}

export class DataStoreReference extends BpmnBaseObject {
  public nodeProcessClass!: String;
  public sourceId!: String;
  public targetId!: String;
}

export class SequenceFlow extends BpmnBaseObject {
  public sourceId!: String;
  public targetId!: String;
}

export class DataOutputAssociation extends BpmnBaseObject {
  public sourceId!: String;
  public targetId!: String;
}

export class DataInputAssociation extends BpmnBaseObject {
  public sourceId!: String;
  public targetId!: String;
}

export class Collaboration extends BpmnBaseObject {}

export const importDiagram = (bpmnJS: any) => (source: Observable<any>) =>
  new Observable<any>((observer) => {
    const subscription = source.subscribe({
      next(xml: any) {
        // canceling the subscription as we are interested
        // in the first diagram to display only
        subscription.unsubscribe();
        console.log(xml);
        bpmnJS.importXML(xml.xml, (err: any, warnings: string | undefined) => {
          if (err) {
            observer.error(err);
          } else {
            observer.next(warnings);
          }

          observer.complete();
        });
      },
      error(e) {
        console.log('ERROR');
        observer.error(e);
      },
      complete() {
        observer.complete();
      },
    });
  });

export interface FormBpmnObject {
  id: string;
  flowid: string;
  flowname: string;
  flowdesc: string;
  flowtype: string;
  nodeProcessClass: string;
  nodeProcessClassVisable: boolean;
  conditionexpression: string;
  conditionexpressionVisable: boolean;
  flowscript: string;
  flowscriptVisable: boolean;
  flowscripttype: string;
  flowscripttypeVisable: boolean;
  nodeProcessParams: string;
}
