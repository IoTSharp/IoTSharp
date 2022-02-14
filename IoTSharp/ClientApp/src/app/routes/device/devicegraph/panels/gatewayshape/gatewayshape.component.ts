import { Component, OnInit } from '@angular/core';
import { IBizData, IShapeData, IToolsPanel } from '../toolspanel';

@Component({
  selector: 'app-gatewayshape',
  templateUrl: './gatewayshape.component.html',
  styleUrls: ['./gatewayshape.component.less']
})
export class GatewayshapeComponent implements OnInit,IToolsPanel {

  constructor() { }
  BizData: IBizData;
  ShapeData: IShapeData;

  ngOnInit(): void {
  }

}
