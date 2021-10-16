import { Component, Injector, OnInit } from '@angular/core';
import pluginForms from 'grapesjs-plugin-forms';
import pluginBlocks from 'grapesjs-blocks-basic';
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
    this.editor = grapesjs.init({
      container: '#gjs',
      showOffsets: 1,
      storageManager: false,
      plugins: ['form'],
      pluginsOpts: {
        'form': {},
      },
      // ...

    });
    //导入布局栏
    pluginBlocks(this.editor, {});
    //导入Form栏
    pluginForms(this.editor, {});



    console.log(customElements.get('nz-select'))
    //导入ng-zorro组件 需要安装@angular/elements支持
    if (!customElements.get('nz-select')) {
      customElements.define('nz-select', createCustomElement(NzSelectComponent, { injector: this.injector }));
    }

    // Input没有专门的Component,再包装一下就行了，AutoComplate也是一样，把属性和事件暴露出来，然后就跟普通Component一样用，difine和add中的名字和你定义的selector一定要保持一致
    if (!customElements.get('app-text-box')) {
      customElements.define('app-text-box', createCustomElement(TextBoxComponent, { injector: this.injector }));
    }


    //样式丢了，结构没有问题
    this._initBlock();
  }

  _initBlock() {



    this.editor.BlockManager.add('nz-select', {
      label: 'nz-select',
      content: `<nz-select ngModel="lucy">
          <nz-option nzValue="jack" nzLabel="Jack"></nz-option>
          <nz-option nzValue="lucy" nzLabel="Lucy"></nz-option>
          <nz-option nzValue="disabled" nzLabel="Disabled" nzDisabled></nz-option>
        </nz-select>
        `,
    });


    this.editor.BlockManager.add('app-text-box', {
      label: 'nz-textbox',
      content: `<app-text-box>
 
        </app-text-box>
        `,
    });
  }
}
