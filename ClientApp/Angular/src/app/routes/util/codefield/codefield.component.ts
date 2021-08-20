import { Component, OnInit } from '@angular/core';
import { ControlWidget } from '@delon/form';

@Component({
  selector: 'app-codefield',
  templateUrl: './codefield.component.html',
  styleUrls: ['./codefield.component.less'],
})
export class CodefieldComponent extends ControlWidget implements OnInit {
  static readonly KEY = 'codefield';
  config: { theme: 'vs-dark'; language: 'csharp' };
  loadingTip: string;
  ngOnInit(): void {
    this.loadingTip = this.ui.loadingTip || '加载中……';
    this.config = this.ui.config || { theme: 'vs-dark', language: 'csharp' };
    this.setValue(this.ui.value);
    this.detectChanges();
    //this.config = { theme: 'vs-dark', language: 'csharp' };
  }
  reset(value: string) {
    //  this.setValue('');
  }
  change(value: string) {
    this.setValue(value);
    if (this.ui.change) this.ui.change(value);
  }
}
