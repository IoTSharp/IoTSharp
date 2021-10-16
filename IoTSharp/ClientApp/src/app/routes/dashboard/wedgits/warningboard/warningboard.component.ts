import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { AbmComponent } from 'angular-baidu-maps';
import { IWidgetComponent } from '../../v1/widgetcomponent';
declare const BMap: any;
@Component({
  selector: 'app-warningboard',
  templateUrl: './warningboard.component.html',
  styleUrls: ['./warningboard.component.less']
})
export class WarningboardComponent implements OnInit,OnDestroy,IWidgetComponent {

  options: any = {};
  bMapLibOptions: any = {};
  status = '';
  @ViewChild('map') mapComp!: AbmComponent;

  private _map: any;
  constructor() { }
  ngOnDestroy(): void {

  }

  ngOnInit(): void {
  }
  onReady(map: any){

    this._map = map;
    map.centerAndZoom(new BMap.Point(116.404, 39.915), 11);
    map.addControl(new BMap.MapTypeControl());
    map.setCurrentCity('北京');
    map.enableScrollWheelZoom(true);
    this.status = '加载完成';
    var point = new BMap.Point(116.404, 39.915);    
    var marker = new BMap.Marker(point);
    map.addOverlay(marker);   
  }
}
