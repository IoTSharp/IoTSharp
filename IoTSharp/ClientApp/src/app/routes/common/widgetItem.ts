import { Type } from '@angular/core';

export class WidgetItem {

  
    constructor(public name: String
      // it should be uniqe
      ,public component: Type<any>, public data: any) {}
  }
  
  
  