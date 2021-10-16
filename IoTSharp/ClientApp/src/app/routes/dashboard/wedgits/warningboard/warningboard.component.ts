import { Component, OnInit } from '@angular/core';
import { IWidgetComponent } from '../../v1/widgetcomponent';

@Component({
  selector: 'app-warningboard',
  templateUrl: './warningboard.component.html',
  styleUrls: ['./warningboard.component.less'],
})
export class WarningboardComponent implements OnInit, IWidgetComponent {
  constructor() {}

  ngOnInit(): void {}
}
