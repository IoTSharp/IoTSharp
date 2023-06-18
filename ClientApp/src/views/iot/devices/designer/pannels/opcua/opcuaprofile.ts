import { opcuamapping } from "../../models/opcuamapping";
export const opcuaprofile={
devnamespace:'opcua',    
name:'opcua网关',
shape:'gateway', 
baseinfoschema:{},
command:{

  toolbar:[],
  contextmenu:[
    { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
    { command: 'editopcuaprop', txt: "编辑属性", icon: "ele-Edit" },
    { command: 'editopcuamapping', txt: "编辑映射", icon: "ele-Edit" },
  ]
},
mappings:[]
}