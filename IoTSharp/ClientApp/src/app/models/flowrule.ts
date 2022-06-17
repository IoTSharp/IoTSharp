export interface flow {
  flowRule: flowrule;
  flowId: string;
  flowname: string;
  flowType: string;
  bpmnid: string;
  nodeProcessClass: string;
  conditionexpression: string;
  nodeProcessMethod: string;
  nodeProcessParams: string;
  nodeProcessScriptType: string;
  nodeProcessScript: string;
  teststatus: number;
}

export interface flowrule {
  ruleId: string;
  name: string;
  ruledesc: string;
  CreatTime: Date;
  rulestatus: number;
  definitionsXml: string;
  flows: flow[];
}
