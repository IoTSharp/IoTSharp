import { createApp } from 'vue';
import pinia from '/@/stores/index';
import App from './App.vue';
import router from './router';
import { directive } from '/@/utils/directive';
import { i18n } from '/@/i18n';
import other from '/@/utils/other';
import '/@/theme/index.scss';
import mitt from 'mitt';
import VueGridLayout from 'vue-grid-layout';
import 'virtual:windi.css'
import formCreate from '@form-create/element-ui'
// @ts-ignore formCreate auto-import element-plus 部分所需要的依赖
import install from '@form-create/element-ui/auto-import'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'


formCreate.use(install)
const app = createApp(App);
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    app.component(key, component)
}
directive(app);
other.elSvg(app);

app.use(pinia).use(router).use(i18n).use(VueGridLayout).use(formCreate).mount('#app');
app.config.globalProperties.mittBus = mitt();
