import { Type } from '@angular/core';
import { Node } from '@antv/x6';
import { PortManager } from '@antv/x6/lib/model/port';
export interface IBizData {
  id: string;
}
export interface IToolsPanel {
  BizData: any;
  ShapeData: IShapeData;
}

export interface IShapeData {}

export class PanelItem<T> {
  constructor(
    public name: String,
    // it should be uniqe
    public component: Type<any>,
    public data: any,
    public instance: T,
    public isselected: boolean
  ) {}
}

export interface DeviceItem extends IBizData {
  devicename?: string;
  type?: string;
  logo?: string;
  image?: string;
  remark?: string;
  prop?: any;
  ports?: any;
  x?: number;
  y?: number;
  width?: number;
  height?: number;
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
export interface PortItem extends IBizData {
  portName?: string;
  portType?: number;
  portPhyType?: number;
  mateData: PortManager.PortMetadata;
  parentRef: Node;
  addr?: string;
  matadata?: any;
}
export interface EdgeItem extends IBizData {
  EdgeName?: string;
  EdgeType?: number;
  mateData: PortManager.PortMetadata;
}
