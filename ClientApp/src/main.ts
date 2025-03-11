import { createApp } from 'vue';
import pinia from '/@/stores/index';
import App from '/@/App.vue';
import router from '/@/router';
import { directive } from '/@/directive/index';
import { i18n } from '/@/i18n/index';
import other from '/@/utils/other';
import formCreate from '@form-create/element-ui';
import mitt from 'mitt';
import install from '@form-create/element-ui/auto-import';
import ElementPlus from 'element-plus';
import '/@/theme/index.scss';
import VueGridLayout from 'vue-grid-layout';
import zhCn from 'element-plus/es/locale/lang/zh-cn';
import { FastCrud } from '@fast-crud/fast-crud';
import '@fast-crud/fast-crud/dist/style.css';
import ui from '@fast-crud/ui-element';
import dayjs from 'dayjs'; // dayjs
import utc  from 'dayjs/plugin/utc';
import timezone  from 'dayjs/plugin/timezone';
formCreate.use(install);

const app = createApp(App);

directive(app);
other.elSvg(app);
app.use(ElementPlus, { locale: zhCn });
app.use(ui);
app.use(FastCrud);
app.use(pinia).use(router).use(ElementPlus).use(i18n).use(VueGridLayout).mount('#app');


dayjs.extend(utc)
dayjs.extend(timezone)
app.config.globalProperties.mittBus = mitt();
app.config.globalProperties.$day = dayjs; //全局挂载