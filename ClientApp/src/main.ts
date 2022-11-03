import { createApp } from 'vue';
import pinia from '/@/stores/index';
import App from './App.vue';
import router from './router';
import { directive } from '/@/utils/directive';
import { i18n } from '/@/i18n';
import other from '/@/utils/other';

import ElementPlus from 'element-plus';
import 'element-plus/dist/index.css';
import '/@/theme/index.scss';
import mitt from 'mitt';
import VueGridLayout from 'vue-grid-layout';
import 'virtual:windi.css'
import formCreate from '@form-create/element-ui'
const app = createApp(App);

directive(app);
other.elSvg(app);

app.use(pinia).use(router).use(ElementPlus, { i18n: i18n.global.t }).use(i18n).use(VueGridLayout).use(formCreate).mount('#app');
app.config.globalProperties.mittBus = mitt();
