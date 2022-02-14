import { Component, OnInit } from '@angular/core';
import { IBizData, IShapeData, IToolsPanel } from '../toolspanel';

@Component({
  selector: 'app-devivceshape',
  templateUrl: './devivceshape.component.html',
  styleUrls: ['./devivceshape.component.less']
})
export class DevivceshapeComponent implements OnInit,IToolsPanel {

  constructor() { }
  BizData: IBizData;
  ShapeData: IShapeData;

  ngOnInit(): void {
  }

}
