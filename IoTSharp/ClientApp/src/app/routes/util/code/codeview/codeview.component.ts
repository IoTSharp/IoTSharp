import { AfterViewInit, Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-codeview',
  templateUrl: './codeview.component.html',
  styleUrls: ['./codeview.component.less'],
})
export class CodeviewComponent implements AfterViewInit {
  @Input()
  editorOptions = { theme: 'vs-dark', language: 'csharp' };
  @Input()
  code: string = 'class A{\n publlic int A{ get;set;}\n}';
  constructor() {}
  ngAfterViewInit(): void {}
}
