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
} from '@angular/core';
import { ModalHelper, _HttpClient } from '@delon/theme'; //test
import { catchError, map, mergeMap } from 'rxjs/operators';
import * as BpmnJS from 'bpmn-js/dist/bpmn-modeler.production.min.js';

import { Observable, throwError, from, fromEvent } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CombineLatestSubscriber } from 'rxjs/internal/observable/combineLatest';

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
  //又菜又爱写
  isCollapsed = false;

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
  @Input() DefinitionsId: number;

  form: FormBpmnObject = {
    Eventid: '',
    Eventname: '',
    Eventdesc: '',
    Eventtype: '',
    NodeProcessClass: '',
    NodeProcessClassVisable: false,
  };
  activity: Activity;
  selectedValue: any;

  public savediagram() {
    this.activity.DefinitionsId = this.DefinitionsId;
    console.log(this.activity);
    from(this.bpmnJS.saveXML({ format: true }))
      .pipe(
        mergeMap((x: any) => {
          return this.http.post('api/manage/workflow/update', {
            Xml: x.xml,
            Biz: JSON.stringify(this.activity),
          });
        }),
      )
      .subscribe();
  }
  constructor(
    private http: _HttpClient,
    private fb: FormBuilder,
    private cd: ChangeDetectorRef,
    private cdr: ChangeDetectorRef,
    private render: Renderer2,
  ) {
    this.activity = new Activity();
    this.activity.Tasks = [];
    this.activity.GateWays = [];
    this.activity.DataInputAssociations = [];
    this.activity.DataOutputAssociations = [];
    this.activity.SequenceFlows = [];
    this.activity.Lane = [];
    this.activity.LaneSet = [];
    this.activity.EndEvents = [];
    this.activity.StartEvents = [];

    this.bpmnJS = new BpmnJS({
      bpmnRenderer: {
        defaultFillColor: '#e6f7ff',
        defaultStrokeColor: '#1890ff',
      },
    });

    this.bpmnJS.on('import.done', ({ error }) => {
      console.log(error);
      if (!error) {
        this.bpmnJS.get('canvas').zoom('fit-viewport');
      }
    });

    this.bpmnJS.on('element.click', (event) => {
      //  this.form.patchValue({ Eventid: event.element.id, Eventname: event.element.businessObject.name });
      console.log(event);
      switch (event.element.type) {
        case 'bpmn:Task':
          var task = this.activity.Tasks.find((x) => x.id == event.element.id);
          if (task) {
            console.log(this.activity.Tasks);
            if (task.BizObject == null) {
              task.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',

                NodeProcessClass: '',

                NodeProcessClassVisable: true,
              };
            }

            task.BizObject.NodeProcessClassVisable = false;
            this.form = task.BizObject;
          }

          break;
        case 'bpmn:EndEvent':
          var endevent = this.activity.EndEvents.find((x) => x.id == event.element.id);
          if (endevent) {
            if (endevent.BizObject === null) {
              endevent.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',

                NodeProcessClass: '',

                NodeProcessClassVisable: true,
              };
            }

            endevent.BizObject.NodeProcessClassVisable = false;
            this.form = endevent.BizObject;
          }

          break;
        case 'bpmn:StartEvent':
          var startevent = this.activity.StartEvents.find((x) => x.id == event.element.id);
          if (startevent) {
            if (startevent.BizObject === null) {
              startevent.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',

                NodeProcessClass: '',

                NodeProcessClassVisable: true,
              };
            }

            startevent.BizObject.NodeProcessClassVisable = false;
            this.form = startevent.BizObject;
          }

          break;
        case 'bpmn:IntermediateThrowEvent':
          var intermediatethrowevent = this.activity.EndEvents.find((x) => x.id == event.element.id);
          if (intermediatethrowevent) {
            if (intermediatethrowevent.BizObject == null) {
              intermediatethrowevent.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',

                NodeProcessClass: '',

                NodeProcessClassVisable: true,
              };
            }

            intermediatethrowevent.BizObject.NodeProcessClassVisable = false;
            this.form = intermediatethrowevent.BizObject;
          }

          break;
        case 'bpmn:ComplexGateway':
          var complexgateway = this.activity.GateWays.find((x) => x.id == event.element.id);
          if (complexgateway) {
            if (complexgateway.BizObject == null) {
              complexgateway.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',

                NodeProcessClass: '',

                NodeProcessClassVisable: true,
              };
            }

            complexgateway.BizObject.NodeProcessClassVisable = true;
            this.form = complexgateway.BizObject;
          }

          break;
        case 'bpmn:ParallelGateway':
          var parallelgteway = this.activity.GateWays.find((x) => x.id == event.element.id);
          if (parallelgteway) {
            if (parallelgteway.BizObject == null) {
              parallelgteway.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }

            parallelgteway.BizObject.NodeProcessClassVisable = false;
            this.form = parallelgteway.BizObject;
          }

          break;
        case 'bpmn:ExclusiveGateway':
          var exclusivegateway = this.activity.GateWays.find((x) => x.id == event.element.id);
          if (exclusivegateway) {
            if (exclusivegateway.BizObject == null) {
              exclusivegateway.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }

            exclusivegateway.BizObject.NodeProcessClassVisable = true;
            this.form = exclusivegateway.BizObject;
          }

          break;
        case 'bpmn:InclusiveGateway':
          var inclusivegateway = this.activity.GateWays.find((x) => x.id == event.element.id);
          if (inclusivegateway) {
            if (inclusivegateway.BizObject == null) {
              inclusivegateway.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',

                NodeProcessClass: '',

                NodeProcessClassVisable: true,
              };
            }

            inclusivegateway.BizObject.NodeProcessClassVisable = true;
            this.form = inclusivegateway.BizObject;
          }

          break;
        case 'bpmn:BusinessRuleTask':
          var businessruletask = this.activity.Tasks.find((x) => x.id == event.element.id);
          if (businessruletask) {
            if (businessruletask.BizObject == null) {
              businessruletask.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }
            businessruletask.BizObject.NodeProcessClassVisable = false;
            this.form = businessruletask.BizObject;
          }

          break;
        case 'bpmn:EventBasedGateway':
          var eventbasedgateway = this.activity.GateWays.find((x) => x.id == event.element.id);
          if (eventbasedgateway) {
            if (eventbasedgateway.BizObject == null) {
              eventbasedgateway.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }

            eventbasedgateway.BizObject.NodeProcessClassVisable = true;
            this.form = eventbasedgateway.BizObject;
          }

          break;
        case 'bpmn:ReceiveTask':
          var receivetask = this.activity.Tasks.find((x) => x.id == event.element.id);
          if (receivetask) {
            if (receivetask.BizObject == null) {
              receivetask.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }

            receivetask.BizObject.NodeProcessClassVisable = false;
            this.form = receivetask.BizObject;
          }

          break;
        case 'bpmn:UserTask':
          var usertask = this.activity.Tasks.find((x) => x.id == event.element.id);
          if (usertask) {
            if (usertask.BizObject == null) {
              usertask.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }
            usertask.BizObject.NodeProcessClassVisable = false;
            this.form = usertask.BizObject;
          }

          break;
        case 'bpmn:IntermediateCatchEvent':
          var intermediatecatchevent = this.activity.EndEvents.find((x) => x.id == event.element.id);
          if (intermediatecatchevent) {
            if (intermediatecatchevent.BizObject == null) {
              intermediatecatchevent.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',

                NodeProcessClass: '',

                NodeProcessClassVisable: true,
              };
            }

            intermediatecatchevent.BizObject.NodeProcessClassVisable = false;
            this.form = intermediatecatchevent.BizObject;
          }

          break;
        case 'bpmn:ServiceTask':
          var servicetask = this.activity.Tasks.find((x) => x.id == event.element.id);
          if (servicetask) {
            if (servicetask.BizObject == null) {
              servicetask.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }
            servicetask.BizObject.NodeProcessClassVisable = false;
            this.form = servicetask.BizObject;
          }

          break;
        case 'bpmn:ManualTask':
          var manualtask = this.activity.Tasks.find((x) => x.id == event.element.id);
          if (manualtask) {
            if (manualtask.BizObject == null) {
              manualtask.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }

            manualtask.BizObject.NodeProcessClassVisable = false;
            this.form = manualtask.BizObject;
          }

          break;
        case 'bpmn:SendTask':
          var sendtask = this.activity.Tasks.find((x) => x.id == event.element.id);
          if (sendtask) {
            if (sendtask.BizObject == null) {
              sendtask.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',

                NodeProcessClass: '',

                NodeProcessClassVisable: true,
              };
            }

            sendtask.BizObject.NodeProcessClassVisable = false;
            this.form = sendtask.BizObject;
          }

          break;
        case 'bpmn:CallActivity':
          var callactivity = this.activity.Tasks.find((x) => x.id == event.element.id);
          if (callactivity) {
            if (callactivity.BizObject == null) {
              callactivity.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }
            callactivity.BizObject.NodeProcessClassVisable = false;
            this.form = callactivity.BizObject;
          }

          break;
        case 'bpmn:SequenceFlow':
          var sequenceflow = this.activity.SequenceFlows.find((x) => x.id == event.element.id);
          if (sequenceflow) {
            if (!sequenceflow.BizObject) {
              sequenceflow.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }
            sequenceflow.BizObject.NodeProcessClassVisable = false;
            this.form = sequenceflow.BizObject;
          }

          break;
        case 'bpmn:Participant':
          var participant = this.activity.Containers.find((x) => x.id == event.element.id);
          if (participant) {
            if (participant.BizObject == null) {
              participant.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }
            participant.BizObject.NodeProcessClassVisable = false;
            this.form = participant.BizObject;
          }

          break;
        case 'bpmn:SubProcess':
          var subprocess = this.activity.Tasks.find((x) => x.id == event.element.id);
          if (subprocess) {
            if (subprocess.BizObject == null) {
              subprocess.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',
                NodeProcessClassVisable: true,
              };
            }

            subprocess.BizObject.NodeProcessClassVisable = false;
            this.form = subprocess.BizObject;
          }

          break;
        case 'bpmn:Collaboration':
          var collaboration = this.activity.Containers.find((x) => x.id == event.element.id);
          if (collaboration) {
            if (collaboration.BizObject == null) {
              collaboration.BizObject = {
                Eventid: '',
                Eventname: '',
                Eventdesc: '',
                Eventtype: '',
                NodeProcessClass: '',

                NodeProcessClassVisable: true,
              };
            }

            collaboration.BizObject.NodeProcessClassVisable = false;
            this.form = collaboration.BizObject;
          }

          break;
      }
      this.cdr.detectChanges();
      this.cd.detectChanges();
    });
    this.bpmnJS.on('element.changed', (event) => {
      if (event.element.type.indexOf('bpmn') !== -1) {
      }
      console.log(event);
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
        default:
          this.DoBaseBpmnObject(event);
          break;
      }
    });
  }

  DoBaseBpmnObject(e: any): void {
    if (e.gfx) {
      var baseBpmnObject = this.activity.BaseBpmnObjects.find((x) => x.id === e.element.businessObject.id);

      if (baseBpmnObject) {
        baseBpmnObject.id = e.element.id;
        baseBpmnObject.bpmntype = e.element.type;
        baseBpmnObject.BizObject.Eventname = e.element.businessObject.name;
        baseBpmnObject.BizObject.Eventid = e.element.id;
      } else {
        baseBpmnObject = new SequenceFlow();
        baseBpmnObject.id = e.element.businessObject.id;
        baseBpmnObject.bpmntype = e.element.type;
        baseBpmnObject.BizObject = {
          Eventid: '',
          Eventname: '',
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: true,
        };
        baseBpmnObject.outgoing = [];
        baseBpmnObject.incoming = [];
        baseBpmnObject.BizObject.Eventname = e.element.businessObject.name;
        baseBpmnObject.BizObject.Eventid = e.element.id;
        this.activity.BaseBpmnObjects = [...this.activity.BaseBpmnObjects, baseBpmnObject];
      }
      console.log(baseBpmnObject);
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
      this.activity.BaseBpmnObjects = this.activity.BaseBpmnObjects.filter((x) => x.id != e.element.id) ?? [];
    }
  }
  DoDataStoreReference(e: any): void {
    if (e.gfx) {
      var dataStoreReference = this.activity.DataStoreReferences.find((x) => x.id === e.element.businessObject.id);

      if (dataStoreReference) {
        dataStoreReference.id = e.element.id;
        dataStoreReference.BizObject.Eventname = e.element.businessObject.name;
        dataStoreReference.BizObject.Eventid = e.element.id;
        dataStoreReference.bpmntype = e.element.type;
      } else {
        dataStoreReference = new SequenceFlow();
        dataStoreReference.id = e.element.businessObject.id;
        dataStoreReference.bpmntype = e.element.type;
        dataStoreReference.BizObject = {
          Eventid: '',
          Eventname: '',
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: true,
        };
        dataStoreReference.outgoing = [];
        dataStoreReference.incoming = [];
        dataStoreReference.BizObject.Eventname = e.element.businessObject.name;
        dataStoreReference.BizObject.Eventid = e.element.id;
        this.activity.DataStoreReferences = [...this.activity.DataStoreReferences, dataStoreReference];
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
      this.activity.DataStoreReferences = this.activity.DataStoreReferences.filter((x) => x.id != e.element.id) ?? [];
    }
  }
  DoContainer(e: any): void {
    if (e.gfx) {
      var container = this.activity.Containers.find((x) => x.id === e.element.businessObject.id);

      if (container) {
        container.id = e.element.id;
        container.BizObject.Eventname = e.element.businessObject.name;
        container.BizObject.Eventid = e.element.id;
        container.bpmntype = e.element.type;
      } else {
        container = new SequenceFlow();
        container.id = e.element.businessObject.id;
        container.bpmntype = e.element.type;
        container.BizObject = {
          Eventid: '',
          Eventname: '',
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: true,
        };
        container.outgoing = [];
        container.incoming = [];
        container.BizObject.Eventname = e.element.businessObject.name;
        container.BizObject.Eventid = e.element.id;
        this.activity.DataStoreReferences = [...this.activity.Containers, container];
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
      this.activity.Containers = this.activity.Containers.filter((x) => x.id != e.element.id) ?? [];
    }
  }
  DoSubProcess(e: any): void {
    if (e.gfx) {
      let subProcess = this.activity.SubProcesses.find((x) => x.id === e.element.businessObject.id);

      if (subProcess) {
        subProcess.id = e.element.id;
        subProcess.bpmntype = e.element.type;
        subProcess.BizObject.Eventname = e.element.businessObject.name;
        subProcess.BizObject.Eventid = e.element.id;
      } else {
        subProcess = new SequenceFlow();
        subProcess.id = e.element.businessObject.id;
        subProcess.bpmntype = e.element.type;
        subProcess.BizObject = {
          Eventid: '',
          Eventname: '',
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: true,
        };
        subProcess.outgoing = [];
        subProcess.incoming = [];
        subProcess.BizObject.Eventname = e.element.businessObject.name;
        subProcess.BizObject.Eventid = e.element.id;
        this.activity.DataStoreReferences = [...this.activity.SubProcesses, subProcess];
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
      this.activity.SubProcesses = this.activity.SubProcesses.filter((x) => x.id != e.element.id) ?? [];
    }
  }

  doSequenceFlow(e: any): void {
    if (e.gfx) {
      let sequenceflow = this.activity.SequenceFlows.find((x) => x.id === e.element.businessObject.id);

      if (sequenceflow) {
        sequenceflow.id = e.element.id;
        sequenceflow.bpmntype = e.element.type;
        sequenceflow.BizObject.Eventname = e.element.businessObject.name;
        sequenceflow.BizObject.Eventid = e.element.id;
      } else {
        sequenceflow = new SequenceFlow();
        sequenceflow.id = e.element.businessObject.id;
        sequenceflow.bpmntype = e.element.type;
        sequenceflow.BizObject = {
          Eventid: '',
          Eventname: '',
          Eventdesc: '',
          Eventtype: '',

          NodeProcessClass: '',

          NodeProcessClassVisable: true,
        };
        sequenceflow.outgoing = [];
        sequenceflow.incoming = [];
        sequenceflow.sourceId = e.element.businessObject.targetRef.id;
        sequenceflow.BizObject.Eventname = e.element.businessObject.name;
        sequenceflow.BizObject.Eventid = e.element.id;
        this.activity.SequenceFlows = [...this.activity.SequenceFlows, sequenceflow];
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
      this.activity.SequenceFlows = this.activity.SequenceFlows.filter((x) => x.id != e.element.id) ?? [];
    }
  }

  doTask(e: any): void {
    if (e.gfx) {
      var task = this.activity.Tasks.find((x) => x.id === e.element.businessObject.id);

      if (task) {
        task.id = e.element.id;
        task.bpmntype = e.element.type;
        task.BizObject.Eventname = e.element.businessObject.name;
        task.BizObject.Eventid = e.element.id;
      } else {
        task = new Task();
        task.id = e.element.id;
        task.bpmntype = e.element.type;
        task.BizObject = {
          Eventid: '',
          Eventname: '',
          Eventdesc: '',
          Eventtype: '',

          NodeProcessClass: '',

          NodeProcessClassVisable: true,
        };
        task.id = e.element.businessObject.id;
        task.BizObject.Eventname = e.element.businessObject.name;
        task.BizObject.Eventid = e.element.id;
        this.activity.Tasks = [...this.activity.Tasks, task];
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
      this.activity.Tasks = this.activity.Tasks.filter((x) => x.id != e.element.id) ?? [];
    }
  }

  doGateWay(e: any): void {
    if (e.gfx) {
      var gateway = this.activity.GateWays.find((x) => x.id === e.element.businessObject.id);

      if (gateway) {
        gateway.id = e.element.id;

        gateway.bpmntype = e.element.type;
        gateway.BizObject.Eventname = e.element.businessObject.name;
      } else {
        gateway = new GateWay();
        gateway.id = e.element.id;
        gateway.bpmntype = e.element.type;
        gateway.BizObject = {
          Eventid: '',
          Eventname: '',
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: true,
        };
        gateway.id = e.element.businessObject.id;
        gateway.outgoing = [];
        gateway.incoming = [];
        gateway.BizObject.Eventname = e.element.businessObject.name;
        gateway.BizObject.Eventid = e.element.id;
        this.activity.GateWays = [...this.activity.GateWays, gateway];
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
      this.activity.GateWays = this.activity.GateWays.filter((x) => x.id != e.element.id) ?? [];
    }
  }

  doStartEvent(e: any): void {
    var startevent = this.activity.StartEvents.find((x) => x.id === e.element.businessObject.id);
    if (e.gfx) {
      if (startevent) {
        startevent.bpmntype = e.element.type;
        startevent.id = e.element.id;
        startevent.BizObject.Eventname = e.element.businessObject.name;
      } else {
        startevent = new BpmnBaseObject();
        startevent.bpmntype = e.element.type;
        startevent.id = e.element.id;
        startevent.BizObject = {
          Eventid: '',
          Eventname: '',
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: true,
        };
        startevent.id = e.element.businessObject.id;
        startevent.outgoing = [];
        startevent.incoming = [];
        startevent.BizObject.Eventname = e.element.businessObject.name;
        startevent.BizObject.Eventid = e.element.id;
        this.activity.StartEvents = [...this.activity.StartEvents, startevent];
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
      this.activity.StartEvents = this.activity.StartEvents.filter((x) => x.id != e.element.id) ?? [];
    }
  }
  doEndEvent(e: any): void {
    var endevent = this.activity.EndEvents.find((x) => x.id === e.element.businessObject.id);
    if (e.gfx) {
      if (endevent) {
        endevent.id = e.element.id;
        endevent.bpmntype = e.element.type;
        endevent.BizObject.Eventname = e.element.businessObject.name;
      } else {
        endevent = new BpmnBaseObject();
        endevent.id = e.element.id;
        endevent.bpmntype = e.element.type;
        endevent.BizObject = {
          Eventid: '',
          Eventname: '',
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: true,
        };
        endevent.id = e.element.businessObject.id;
        endevent.outgoing = [];
        endevent.incoming = [];
        endevent.BizObject.Eventname = e.element.businessObject.name;
        endevent.BizObject.Eventid = e.element.id;
        this.activity.EndEvents = [...this.activity.EndEvents, endevent];
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
      this.activity.EndEvents = this.activity.EndEvents.filter((x) => x.id != e.element.id) ?? [];
    }
  }

  ngAfterContentInit(): void {
    this.bpmnJS.attachTo(this.el.nativeElement);
    this.render.setStyle(this.el.nativeElement, 'height', window.innerHeight - 64 + 'px');
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.url) {
      this.loadUrl(changes.url.currentValue);
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
      console.log('load');
      this.activity = new Activity();
      this.activity.SequenceFlows = [];
      this.activity.Tasks = [];
      this.activity.GateWays = [];
      this.activity.Lane = [];
      this.activity.LaneSet = [];
      this.activity.EndEvents = [];
      this.activity.StartEvents = [];
      this.activity.BaseBpmnObjects = [];
      this.activity.DataStoreReferences = [];
      this.activity.SubProcesses = [];
      this.activity.DataOutputAssociations = [];
      this.activity.DataInputAssociations = [];
      this.activity.DefinitionsDesc = '';
      this.activity.DefinitionsId = 0;
      this.activity.DefinitionsName = '';
      this.activity.DefinitionsStatus = 1;
    } else {
      this.http.get<DesignerResult>(url).subscribe(
        async (data) => {
          this.activity = new Activity();
          this.activity.SequenceFlows = [];
          this.activity.Tasks = [];
          this.activity.GateWays = [];
          this.activity.Lane = [];
          this.activity.LaneSet = [];
          this.activity.EndEvents = [];
          this.activity.StartEvents = [];
          this.activity.BaseBpmnObjects = [];
          this.activity.DataStoreReferences = [];
          this.activity.SubProcesses = [];
          this.activity.DataOutputAssociations = [];
          this.activity.DataInputAssociations = [];
          this.activity.DefinitionsDesc = '';
          this.activity.DefinitionsId = 0;
          this.activity.DefinitionsName = '';
          this.activity.DefinitionsStatus = 1;
          await this.bpmnJS.importXML(data.Xml);
          this.InitData(data.Biz);
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
    if (data.GateWays) {
      for (var element of data.GateWays) {
        var gateWay = new GateWay();
        gateWay.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: element.BizObject.NodeProcessClass,
          NodeProcessClassVisable: true,
        };
        gateWay.incoming = element.incoming ?? [];
        gateWay.outgoing == element.incoming ?? [];
        gateWay.id = element.id;
        gateWay.bpmntype = element.bpmntype;
        this.activity.GateWays.push(gateWay);
      }
    }
    if (data.SequenceFlows) {
      for (var element of data.SequenceFlows) {
        var sequenceflows = new GateWay();
        sequenceflows.BizObject = {
          Eventid: element.id,
          Eventname: element.Eventname,
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: false,
        };
        sequenceflows.incoming = element.incoming ?? [];
        sequenceflows.outgoing = element.incoming ?? [];
        sequenceflows.id = element.id;
        sequenceflows.bpmntype = element.bpmntype;
        this.activity.SequenceFlows.push(sequenceflows);
      }
    }
    if (data.Tasks) {
      for (var element of data.Tasks) {
        var task = new Task();
        task.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: false,
        };
        task.incoming = element.incoming ?? [];
        task.outgoing = element.incoming ?? [];
        task.id = element.id;
        task.bpmntype = element.bpmntype;
        this.activity.Tasks.push(task);
      }
    }

    if (data.LaneSet) {
      for (var element of data.LaneSet) {
        var laneset = new BpmnBaseObject();
        laneset.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: false,
        };
        laneset.incoming = element.incoming ?? [];
        laneset.outgoing = element.incoming ?? [];
        laneset.id = element.id;
        laneset.bpmntype = element.bpmntype;
        this.activity.LaneSet.push(laneset);
      }
    }
    if (data.EndEvents) {
      for (var element of data.EndEvents) {
        var endevent = new BpmnBaseObject();
        endevent.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',

          NodeProcessClass: '',

          NodeProcessClassVisable: false,
        };
        endevent.incoming = element.incoming ?? [];
        endevent.outgoing = element.incoming ?? [];
        endevent.id = element.id;
        endevent.bpmntype = element.bpmntype;
        this.activity.EndEvents.push(endevent);
      }
    }

    if (data.StartEvents) {
      for (var element of data.StartEvents) {
        var startevent = new BpmnBaseObject();
        startevent.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',

          NodeProcessClass: '',

          NodeProcessClassVisable: false,
        };
        startevent.incoming = [];
        startevent.outgoing = [];
        startevent.id = element.id;
        startevent.bpmntype = element.bpmntype;
        this.activity.StartEvents.push(startevent);
      }
    }

    if (data.Containers) {
      for (var element of data.Containers) {
        var container = new BpmnBaseObject();
        container.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: false,
        };
        container.incoming = element.incoming ?? [];
        container.outgoing = element.incoming ?? [];
        container.id = element.id;
        container.bpmntype = element.bpmntype;
        this.activity.Containers.push(container);
      }
    }

    if (data.BaseBpmnObjects) {
      for (var element of data.BaseBpmnObjects) {
        var baseBpmnObject = new BpmnBaseObject();
        baseBpmnObject.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',

          NodeProcessClassVisable: false,
        };
        baseBpmnObject.incoming = element.incoming ?? [];
        baseBpmnObject.outgoing = element.incoming ?? [];
        baseBpmnObject.id = element.id;
        baseBpmnObject.bpmntype = element.bpmntype;
        this.activity.BaseBpmnObjects.push(baseBpmnObject);
      }
    }

    if (data.DataStoreReferences) {
      for (var element of data.DataStoreReferences) {
        var datastorereference = new BpmnBaseObject();
        datastorereference.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: false,
        };
        datastorereference.incoming = element.incoming ?? [];
        datastorereference.outgoing = element.incoming ?? [];
        datastorereference.id = element.id;
        datastorereference.bpmntype = element.bpmntype;
        this.activity.DataStoreReferences.push(datastorereference);
      }
    }
    if (data.SubProcesses) {
      for (var element of data.SubProcesses) {
        var subprocess = new BpmnBaseObject();
        subprocess.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: false,
        };
        subprocess.incoming = element.incoming ?? [];
        subprocess.outgoing = element.incoming ?? [];
        subprocess.id = element.id;
        subprocess.bpmntype = element.bpmntype;
        this.activity.SubProcesses.push(subprocess);
      }
    }

    if (data.DataOutputAssociations) {
      for (var element of data.DataOutputAssociations) {
        var dataOutputAssociation = new DataOutputAssociation();
        dataOutputAssociation.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: false,
        };
        dataOutputAssociation.incoming = element.incoming ?? [];
        dataOutputAssociation.outgoing = element.incoming ?? [];
        dataOutputAssociation.id = element.id;
        dataOutputAssociation.bpmntype = element.bpmntype;
        this.activity.DataOutputAssociations.push(dataOutputAssociation);
      }
    }

    if (data.DataInputAssociations) {
      for (var element of data.DataInputAssociations) {
        var dataInputAssociations = new DataOutputAssociation();
        dataInputAssociations.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',

          NodeProcessClass: '',

          NodeProcessClassVisable: false,
        };
        dataInputAssociations.incoming = element.incoming ?? [];
        dataInputAssociations.outgoing = element.incoming ?? [];
        dataInputAssociations.id = element.id;
        dataInputAssociations.bpmntype = element.bpmntype;
        this.activity.DataInputAssociations.push(dataInputAssociations);
      }
    }

    if (data.Lane) {
      for (var element of data.Lane) {
        var lane = new BpmnBaseObject();
        lane.BizObject = {
          Eventid: element.id,
          Eventname: element.BizObject.Eventname,
          Eventdesc: '',
          Eventtype: '',
          NodeProcessClass: '',
          NodeProcessClassVisable: false,
        };
        lane.incoming = element.incoming ?? [];
        lane.outgoing = element.incoming ?? [];
        lane.id = element.id;
        lane.bpmntype = element.bpmntype;
        this.activity.Lane.push(lane);
      }
    }
  }
}

export class BaseBizObject {
  Eventid!: String;
  Eventname!: String;
}

export class DesignerResult {
  public Biz!: Activity;
  public Xml!: String;
}
export class BpmnBaseObject {
  public id!: String;
  public bpmntype!: String;
  public incoming!: BpmnBaseObject[];
  public outgoing!: BpmnBaseObject[];
  public BizObject: FormBpmnObject = {
    Eventid: '',
    Eventname: '',
    Eventdesc: '',
    Eventtype: '',
    NodeProcessClass: '',
    NodeProcessClassVisable: true,
  };
}

export class Activity {
  public SequenceFlows!: SequenceFlow[];
  public Tasks!: Task[];
  public GateWays!: GateWay[];
  public Lane!: BpmnBaseObject[];
  public LaneSet!: BpmnBaseObject[];
  public EndEvents!: BpmnBaseObject[];
  public StartEvents!: BpmnBaseObject[];
  public Containers!: BpmnBaseObject[];
  public BaseBpmnObjects!: BpmnBaseObject[];
  public DataStoreReferences!: BpmnBaseObject[];
  public SubProcesses!: BpmnBaseObject[];
  public DataOutputAssociations!: DataOutputAssociation[];
  public DataInputAssociations!: DataOutputAssociation[];
  public DefinitionsDesc!: String;
  public DefinitionsId!: Number;
  public DefinitionsName!: String;
  public DefinitionsStatus!: Number;
}

export class Task extends BpmnBaseObject {
  public Eventtype!: String;
  public FlowId!: Number;
}
export class GateWay extends BpmnBaseObject {
  public NodeProcessClass!: String;
  public sourceId!: String;
  public targetId!: String;
}

export class DataStoreReference extends BpmnBaseObject {
  public NodeProcessClass!: String;
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
        bpmnJS.importXML(xml.Xml, (err: any, warnings: string | undefined) => {
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

export class FormBpmnObject {
  public Eventid!: string;
  public Eventname!: string;
  public Eventdesc!: string;
  public Eventtype!: string;
  public NodeProcessClass!: string;
  public NodeProcessClassVisable: boolean = true;
}
