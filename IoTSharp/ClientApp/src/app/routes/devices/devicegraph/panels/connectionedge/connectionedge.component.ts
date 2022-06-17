import { Component, OnInit } from '@angular/core';
import { IBizData, IShapeData, IToolsPanel } from 'src/app/models/devicegraph/ibizdata';

@Component({
  selector: 'app-connectionedge',
  templateUrl: './connectionedge.component.html',
  styleUrls: ['./connectionedge.component.less']
})
export class ConnectionedgeComponent implements OnInit, IToolsPanel {
  constructor() {}
  BizData: IBizData;
  ShapeData: IShapeData;

  ngOnInit(): void {}
}
