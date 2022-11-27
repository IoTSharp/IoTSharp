export interface GataWayItem {

}


export interface DeviceItem {

}

export interface GatewayDesignerState {
    isShow: boolean; leftNavList: any[];
}





export interface NavItem {
    title?: string;
    id: string;
    icon: string
    isOpen:boolean
    children: NavItem[]
}
