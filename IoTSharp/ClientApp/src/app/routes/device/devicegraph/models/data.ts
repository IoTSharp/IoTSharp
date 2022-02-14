export interface DeviceItem {
    devicename: string;
    id: string;
    type: string;
    logo: string;
    image: string;
    remark: string;
    prop: any;
    ports: any;
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
  export interface port {
    portid: number;
    portName: string;
    portType: number;
    portPhyType: number;
  }
  