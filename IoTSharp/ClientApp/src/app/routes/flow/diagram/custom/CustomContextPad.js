import {
    TASKS,
} from './Parts'
export default class CustomContextPad {
    constructor(bpmnFactory, config, contextPad, create, elementFactory, injector, translate) {
        this.bpmnFactory = bpmnFactory;
        this.create = create;
        this.elementFactory = elementFactory;
        this.translate = translate;
        if (config.autoPlace !== false) {
            this.autoPlace = injector.get('autoPlace', false);
        }
        contextPad.registerProvider(this);
    }

    getContextPadEntries(element) {
        const {
            autoPlace,
            bpmnFactory,
            create,
            elementFactory,
        } = this;

        function appendBuiltinShape(task) {
            return function (event, element) {
                if (autoPlace) {
                    const businessObject = bpmnFactory.create(task.shape);
                    businessObject.suitable = task.title; 
                    businessObject.profile = task;
                    const shape = elementFactory.createShape({
                        type: task.shape,
                        businessObject: businessObject,
                    });     
                 
                    autoPlace.append(element, shape);
                } else {
                    appendBuiltinShapeStart(event, element);
                }
            };
        }
        function appendBuiltinShapeStart(task) {
            return function (event) {
                const businessObject = bpmnFactory.create(task.shape);
                businessObject.suitable = task.title; 
                businessObject.profile = task;
     
                const shape = elementFactory.createShape({
                    type: task.shape,
                    businessObject: businessObject,
                });
       console.log(shape)
                create.start(event, shape, element);
            };
        }
        var tasks = {}
        for (var o of TASKS) {
            tasks[o.namespace] = {
                group: o.group,
                className: o.classname,
                title: o.desc,
                action: {
                    click: appendBuiltinShape(o),
                    dragstart: appendBuiltinShapeStart(o)
                }
            }

        }
        return tasks;
    }
}

CustomContextPad.$inject = [
    'bpmnFactory',
    'config',
    'contextPad',
    'create',
    'elementFactory',
    'injector',
    'translate'
];