import { Component, OnInit } from '@angular/core';
import { IBizData, IShapeData, IToolsPanel } from '../toolspanel';

@Component({
  selector: 'app-portshape',
  templateUrl: './portshape.component.html',
  styleUrls: ['./portshape.component.less']
})
export class PortshapeComponent implements OnInit ,IToolsPanel{

  constructor() { }
  BizData: IBizData;
  ShapeData: IShapeData;

  ngOnInit(): void {
  }

}
