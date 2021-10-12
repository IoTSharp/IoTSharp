import { ChangeDetectorRef, Component, ComponentFactory, ComponentFactoryResolver, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { CdkDragDrop, moveItemInArray, transferArrayItem, CdkDrag } from '@angular/cdk/drag-drop';
import { _HttpClient } from '@delon/theme';

@Component({
  selector: 'app-dynamicformdesignerv2',
  templateUrl: './dynamicformdesignerv2.component.html',
  styleUrls: ['./dynamicformdesignerv2.component.less']
})
export class Dynamicformdesignerv2Component implements OnInit {

  //简化可控的设计器


  isCollapsed = false;
  all = [1, 2, 3, 4, 5, 6, 7, 8, 9];
  even = [1];

  constructor() { }

  ngOnInit(): void {

  }
  drop(event: CdkDragDrop<any>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex);
    }
  }

  /** Predicate function that only allows even numbers to be dropped into a list. */
  evenPredicate(item: CdkDrag<any>) {
    return item.data;
  }

  /** Predicate function that doesn't allow items to be dropped into a list. */
  noReturnPredicate() {
    return false;
  }


}
