import BaseRenderer from 'diagram-js/lib/draw/BaseRenderer';

import {
  append as svgAppend,
  attr as svgAttr,
  classes as svgClasses,
  create as svgCreate
} from 'tiny-svg';

import {
  getRoundRectPath
} from 'bpmn-js/lib/draw/BpmnRenderUtil';

import {
  is,
  getBusinessObject
} from 'bpmn-js/lib/util/ModelUtil';

import { isNil } from 'min-dash';

const HIGH_PRIORITY = 1500,
      TASK_BORDER_RADIUS = 3



export default class CustomRenderer extends BaseRenderer {
  constructor(eventBus, bpmnRenderer) {
    super(eventBus, HIGH_PRIORITY);
    this.bpmnRenderer = bpmnRenderer;
  }

  canRender(element) {
    return !element.labelTarget;
  }

  drawShape(parentNode, element) {
    const shape = this.bpmnRenderer.drawShape(parentNode, element);
    const profile = this.getSuitabilityProfile(element);
    if (!isNil(profile)) {
      const color = profile.color;
      const rect = drawRect(parentNode, 120, 20, TASK_BORDER_RADIUS, color);
      svgAttr(rect, {
        transform: 'translate(0, -25)'
      });
      var text = svgCreate('text');
      svgAttr(text, {
        fill: '#fff',
        transform: 'translate(0, -10)'
      });
      svgClasses(text).add('djs-label');
      svgAppend(text, document.createTextNode(profile.title));
      svgAppend(parentNode, text);
    }
    return shape;
  }

  getShapePath(shape) {
    return getRoundRectPath(shape, TASK_BORDER_RADIUS);
  }

  getSuitabilityProfile(element) {
    const businessObject = getBusinessObject(element);
    const { suitable ,profile} = businessObject;
    return profile;
  }

}

function drawRect(parentNode, width, height, borderRadius, color) {
  const rect = svgCreate('rect');
  svgAttr(rect, {
    width: width,
    height: height,
    rx: borderRadius,
    ry: borderRadius,
    stroke: color,
    strokeWidth: 1,
    fill: color
  });
  svgAppend(parentNode, rect);
  return rect;
} 