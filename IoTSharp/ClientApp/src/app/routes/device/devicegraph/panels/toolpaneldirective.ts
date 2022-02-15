import { Directive, ViewContainerRef } from '@angular/core';
@Directive({
    selector: '[toolpanelcontainer]',
  })
export class toolpaneldirective {
    constructor(public viewContainerRef: ViewContainerRef) { }
  }
  