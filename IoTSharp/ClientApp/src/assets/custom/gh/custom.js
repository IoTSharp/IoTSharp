import CustomContextPad from './customContextPad';
import CustomPalette from './customPalette';
import CustomRenderer from './customRenderer';

export default {
    __init__: [ 'customContextPad', 'customPalette', 'customRenderer' ],
    customContextPad: [ 'type', CustomContextPad ],
    customPalette: [ 'type', CustomPalette ],
    customRenderer: [ 'type', CustomRenderer ]
  }; 