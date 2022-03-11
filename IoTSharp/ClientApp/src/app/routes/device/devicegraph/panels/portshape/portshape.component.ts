import { Component, OnInit } from '@angular/core';
import { PortItem } from '../../models/data';
import { IBizData, IShapeData, IToolsPanel } from '../toolspanel';

@Component({
  selector: 'app-portshape',
  templateUrl: './portshape.component.html',
  styleUrls: ['./portshape.component.less']
})
export class PortshapeComponent implements OnInit ,IToolsPanel{

  constructor() { }
  BizData: PortItem;
  ShapeData: IShapeData;

  ngOnInit(): void {

    

  }

}
