export interface devicemodel{
  deviceModelId:string;
  modelName:string;
  modelDesc:string;
  modelStatus:string;
  createDateTime:string;
  creator:string;
  deviceModelCommands:devicemodelcommand[];
}

export interface devicemodelcommand{

  commandId:string;
  commandTitle:string;
  commandI18N:string;
  commandType:string;
  commandParams:string;
  commandName:string;
  commandTemplate:string;
  deviceModelId:string;
  commandStatus:string;

}


export  interface devicemodelcommandparam{
  commandId:string;
  deviceModelId:string;
}
