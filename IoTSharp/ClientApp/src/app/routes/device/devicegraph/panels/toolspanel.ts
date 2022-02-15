export interface IToolsPanel {
    BizData:IBizData  
    ShapeData:IShapeData  
  }
  

  export interface IShapeData {
  
  }


  export interface IBizData {
  
  }

  import { Type } from '@angular/core';

export class PanelItem<IToolsPanel> {

  
    constructor(public name: String
      // it should be uniqe
      ,public component: Type<any>, public data: any) {}
  }
  
  
  