import { opcuamapping } from "../../models/opcuamapping";
export const deviceprofile={
devnamespace:'iot.device.test',   
shape:'device', 
name:'测试设备',
baseinfoschema:{},
command:{

  toolbar:[],
  contextmenu:[
    { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
    { command: 'editmodbusmapping', txt: "编辑", icon: "ele-Edit" },
  ]
},
mappings:[]
}