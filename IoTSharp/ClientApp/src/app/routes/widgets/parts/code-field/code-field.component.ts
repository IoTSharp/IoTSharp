import { Component, OnInit } from '@angular/core';
import { ControlWidget } from '@delon/form';

@Component({
  selector: 'app-code-field',
  templateUrl: './code-field.component.html',
  styleUrls: ['./code-field.component.less']
})
export class CodeFieldComponent extends ControlWidget implements OnInit {
  static readonly KEY = 'codefield';
  config: { theme: 'vs-dark'; language: 'csharp' };
  loadingTip: string;
  ngOnInit(): void {
    this.loadingTip = this.ui['loadingTip'] || '加载中……';
    this.config = this.ui['config'] || { theme: 'vs-dark', language: 'csharp' };
    this.setValue(this.ui['value']);
    this.detectChanges();
    //this.config = { theme: 'vs-dark', language: 'csharp' };
  }
  override reset(value: string) {
    //  this.setValue('');
  }
  change(value: string) {
    this.setValue(value);
    if (this.ui['change']) this.ui['change'](value);
  }
}
