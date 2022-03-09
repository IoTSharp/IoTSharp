import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { DeviceItem } from '../../models/data';
import { IBizData, IShapeData, IToolsPanel } from '../toolspanel';

@Component({
  selector: 'app-gatewayshape',
  templateUrl: './gatewayshape.component.html',
  styleUrls: ['./gatewayshape.component.less']
})
export class GatewayshapeComponent implements OnInit,IToolsPanel {

  constructor(private cdr: ChangeDetectorRef ) { }
  BizData: DeviceItem={ id:'-1' ,x:11};
  ShapeData: IShapeData;
  ngOnInit(): void {
    console.log(this.BizData)
    this.cdr.detectChanges();
  }

}
