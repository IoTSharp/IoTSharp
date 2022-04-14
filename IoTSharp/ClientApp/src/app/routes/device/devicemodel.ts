import { number } from "echarts";

export interface deviceitem {
    deviceType?: string;
    id?: string;
    lastActive?: string;
    name?: string;
    online?: string;
    owner?: string;
    tenant?: string;
    identityId?: string;
    timeout?: string;
    customerId?: string;
    telemetries?: telemetryitem[];
    attributes?: attributeitem[];
    rules?: ruleitem[];
    expand?: boolean;
    identityType?: string;
    identityValue?: string;
  }
  
  export interface telemetryitem {
    keyName: string;
    dateTime: string;
    value: any;
    class?:string;
    variation:any;
    average:any|number;
    sum:any|number;
    min:any|number;
    max:any|number;
    checked?:any|boolean;
  }
  export interface attributeitem {
    keyName: string;
    dataSide: string;
    dateTime: string;
    value: any;
    class?: string;
  }
  
  export interface ruleitem {
    ruleId: number;
    name: string;
    ruleDesc: string;
    describes: string;
  }
  