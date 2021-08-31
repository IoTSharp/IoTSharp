import { ChangeDetectorRef, Component, Input, OnInit, ViewChild } from '@angular/core';

@Component({
  selector: 'app-dynamicformresultview',
  templateUrl: './dynamicformresultview.component.html',
  styleUrls: ['./dynamicformresultview.component.less'],
})
export class DynamicformresultviewComponent implements OnInit {
  @Input()
  result: any;
  constructor() {}

  ngOnInit(): void {}
}
