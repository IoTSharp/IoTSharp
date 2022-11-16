// 定义接口来定义对象的类型
export interface NodeListState {
    id: string | number;
    nodeId: string | undefined;
    class: HTMLElement | string;
    left: number | string;
    top: number | string;
    icon: string;
    name: string;
    nodetype?: string;
    namespace?: string;
    mata?: string;
    content?: string;
    color: string; // 节点背景颜色
}
export interface LineListState {
    sourceId: string;
    targetId: string;
    label: string;
    condition?: string;
    linename?: string;
    lineId: string;
}
export interface XyState {
    x: string | number;
    y: string | number;
}
export interface FlowState {
    flowid?: string | any;
    // workflowRightRef: HTMLDivElement | null;
    // leftNavRefs: any[];
    leftNavList: any[];
    dropdownNode: XyState;
    dropdownLine: XyState;
    isShow: boolean;
    jsPlumb: any;
    jsPlumbNodeIndex: null | number;
    jsplumbDefaults: any;
    jsplumbMakeSource: any;
    jsplumbMakeTarget: any;
    jsplumbConnect: any;
    jsplumbData: {
        nodeList: Array<NodeListState>;
        lineList: Array<LineListState>;
    };
}
