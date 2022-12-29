export interface GataWayItem {

}


export interface DeviceItem {

}

export interface GatewayDesignerState {
    isShow: boolean; leftNavList: any[];dropdownNode:XyState
}


export interface XyState {
    x: string | number;
    y: string | number;
}


export interface NavItem {
    title?: string;
    id: string;
    icon: string
    isOpen:boolean
    children: NavItem[]
}
