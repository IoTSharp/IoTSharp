
import { modbusmapping } from "../../models/modbusmapping";
import { v4 as uuidv4, NIL as NIL_UUID } from "uuid";
export const modbusprofile={
devnamespace:'modbus',    
name:'ModBus网关',
shape:'gateway', 
baseinfoschema:{},
command:{

  toolbar:[],
  contextmenu:[
    { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
    { command: 'editmodbusprop', txt: "编辑属性", icon: "ele-Edit" },
    { command: 'editmodbusmapping', txt: "编辑映射", icon: "ele-Edit" },
  ],
},
mappings: []
}