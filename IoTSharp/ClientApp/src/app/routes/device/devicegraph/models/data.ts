import { Node } from "@antv/x6";
import { PortManager } from "@antv/x6/lib/model/port";
import { IBizData } from "../panels/toolspanel";

export interface DeviceItem extends IBizData {
    devicename?: string;
    type?: string;
    logo?: string;
    image?: string;
    remark?: string;
    prop?: any;
    ports?: any;
    x?:number;
    y?:number;
    width?:number;
    height?:number;
    mateData?: any;

  }
  export interface DeviceInfo {
    Income: string[];
    OutGoing: string[];
    Label: string;
    LocationX: number;
    LocationY: number;
    Width: number;
    Height: number;
    Type: string;
  }
  export interface PortItem extends IBizData  {
    portName?: string;
    portType?: number;
    portPhyType?: number;
    mateData: PortManager.PortMetadata
    parentRef:Node;
    addr?: string;
    matadata?: any;

  }
  export interface EdgeItem extends IBizData  {
    EdgeName?: string;
    EdgeType?: number;
    mateData: PortManager.PortMetadata

  }
  