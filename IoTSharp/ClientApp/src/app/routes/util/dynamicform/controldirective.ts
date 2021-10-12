import { Directive, Type, ViewContainerRef } from '@angular/core';
@Directive({
    selector: '[controlcontainer]',
})
export class controldirective {
    constructor(public viewContainerRef: ViewContainerRef) { }
}
