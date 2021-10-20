import { Directive, ViewContainerRef } from '@angular/core';
@Directive({
    selector: '[widgetcontainer]',
  })
  export class widgetdirective {
    constructor(public viewContainerRef: ViewContainerRef) { }
  }
  