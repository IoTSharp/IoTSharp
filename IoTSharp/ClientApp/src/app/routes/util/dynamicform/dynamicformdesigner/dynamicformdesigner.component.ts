import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dynamicformdesigner',
  templateUrl: './dynamicformdesigner.component.html',
  styleUrls: ['./dynamicformdesigner.component.less'],
})
export class DynamicformdesignerComponent implements OnInit {
  constructor() {}
  editor;
  ngOnInit(): void {
    this.editor = grapesjs.init({
      container: '#gjs',
      // ...
      blockManager: {
        appendTo: '#blocks',
        blocks: [
          {
            id: 'section', // id is mandatory
            label: '<b>Section</b>', // You can use HTML/SVG inside labels
            attributes: { class: 'gjs-block-section' },
            content: `<section>
          <h1>This is a simple title</h1>
          <div>This is just a Lorem text: Lorem ipsum dolor sit amet</div>
        </section>`,
          },
          {
            id: 'text',
            label: 'Text',
            content: '<div data-gjs-type="text">Insert your text here</div>',
          },
          {
            id: 'image',
            label: 'Image',
            // Select the component once it's dropped
            select: true,
            // You can pass components as a JSON instead of a simple HTML string,
            // in this case we also use a defined component type `image`
            content: { type: 'image' },
            // This triggers `active` event on dropped components and the `image`
            // reacts by opening the AssetManager
            activate: true,
          },
        ],
      },
    });

    this._initBlock();
  }

  _initBlock() {
    this.editor.BlockManager.add('my-first-block', {
      label: 'Simple block',
      content: '<div class="my-block">This is a simple block</div>',
    });
  }
}
