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
      profile: {}
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
    profile?: any;
  }
  