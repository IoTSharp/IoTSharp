
import { modbusmapping } from "../../models/modbusmapping";
import { v4 as uuidv4, NIL as NIL_UUID } from "uuid";
export const modbusprofile={
devnamespace:'modbus',    
name:'ModBus网关',
baseinfoschema:{},
command:{

  toolbar:[],
  contextmenu:[
    { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
    { command: 'editmodbusmapping', txt: "编辑", icon: "ele-Edit" },
  ],
},
mappings: []
}