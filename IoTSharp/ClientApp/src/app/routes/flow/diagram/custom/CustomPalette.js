import {
  TASKS,
} from './Parts'
export default class CustomPalette {
  constructor(bpmnFactory, create, elementFactory, palette, translate) {
    this.bpmnFactory = bpmnFactory;
    this.create = create;
    this.elementFactory = elementFactory;
    this.translate = translate;

    palette.registerProvider(this);
  }

  getPaletteEntries(element) {
    const {
      bpmnFactory,
      create,
      elementFactory,
    } = this;

    function createbuiltinshape(task) {
      return function (event) {
        const businessObject = bpmnFactory.create(task.shape);
        businessObject.suitable = task.title; 
        businessObject.profile = task;
        const shape = elementFactory.createShape({
          type: task.shape,
          businessObject: businessObject
        });
        console.log(shape)
        create.start(event, shape);
      };
    }
    var tasks = {
      'excutors-separator': {
        group: 'excutors',
        separator: true
      },
    };
    for (var o of TASKS) {
      tasks[o.namespace] = {
        group: o.group,
        className: o.classname,
        title: o.desc,
        action: {
          dragstart: createbuiltinshape(o),
          click: createbuiltinshape(o)
        }
      }

    }
    return tasks;
  }
}

CustomPalette.$inject = [
  'bpmnFactory',
  'create',
  'elementFactory',
  'palette',
  'translate'
];