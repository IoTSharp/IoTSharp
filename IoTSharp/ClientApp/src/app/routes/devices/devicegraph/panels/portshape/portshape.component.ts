import { Component, OnInit } from '@angular/core';
import { IShapeData, PortItem } from 'src/app/models/devicegraph/ibizdata';

@Component({
  selector: 'app-portshape',
  templateUrl: './portshape.component.html',
  styleUrls: ['./portshape.component.less']
})
export class PortshapeComponent implements OnInit {
  constructor() {}
  BizData: PortItem;
  ShapeData: IShapeData;

  ngOnInit(): void {}
}
