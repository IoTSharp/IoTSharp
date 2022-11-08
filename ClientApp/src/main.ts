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

// ! 如果您使用 unplugin-element-plus 并且只使用组件 API，您需要手动导入样式。
// ! 参考官方文档 https://element-plus.gitee.io/zh-CN/guide/quickstart.html#%E6%89%8B%E5%8A%A8%E5%AF%BC%E5%85%A5
import 'element-plus/es/components/message/style/css'
import 'element-plus/es/components/notification/style/css'
import 'element-plus/es/components/message-box/style/css'

formCreate.use(install)
const app = createApp(App);
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    app.component(key, component)
}
directive(app);
other.elSvg(app);

app.use(pinia).use(router).use(i18n).use(VueGridLayout).use(formCreate).mount('#app');
app.config.globalProperties.mittBus = mitt();
