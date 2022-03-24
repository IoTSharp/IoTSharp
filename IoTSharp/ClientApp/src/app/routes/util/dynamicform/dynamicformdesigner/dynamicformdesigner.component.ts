import { Component, Injector, OnInit } from '@angular/core';

import { createCustomElement } from '@angular/elements';
import { NzSelectComponent } from 'ng-zorro-antd/select';
import { TextBoxComponent } from '../cps/text-box/text-box.component';


@Component({
  selector: 'app-dynamicformdesigner',
  templateUrl: './dynamicformdesigner.component.html',
  styleUrls: ['./dynamicformdesigner.component.less'],
})
export class DynamicformdesignerComponent implements OnInit {
  constructor(private injector: Injector) { }
  editor;
  ngOnInit(): void {
   
     
    
  }

  

}
