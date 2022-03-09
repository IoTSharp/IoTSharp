export interface IToolsPanel {
    BizData:any  
    ShapeData:IShapeData  
  }
  

  export interface IShapeData {
   
    
  
  }


  export interface IBizData {
    id:string
  }

  import { Type } from '@angular/core';

export class PanelItem<T> {
    constructor(public name: String
      // it should be uniqe
      ,public component: Type<any>, public data: any,public  instance:T, public isselected:boolean) {}
  }
  
  
  