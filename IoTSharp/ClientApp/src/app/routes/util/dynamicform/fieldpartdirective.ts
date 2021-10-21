import { Directive, Type, ViewContainerRef } from '@angular/core';
@Directive({
  selector: '[fieldcontainer]',
})
export class fielddirective {
  constructor(public viewContainerRef: ViewContainerRef) {}
}
