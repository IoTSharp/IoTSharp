import { createApp } from 'vue';
import pinia from '/@/stores/index';
import App from '/@/App.vue';
import router from '/@/router';
import { ensureRouteFeatureInstall, setupRouteFeatureGuards } from '/@/bootstrap/routeFeatures';
import { directive } from '/@/directive/index';
import { i18n } from '/@/i18n/index';
import mitt from 'mitt';
import ElementPlus from 'element-plus';
import 'element-plus/dist/index.css';
import '/@/theme/index.scss';
import zhCn from 'element-plus/es/locale/lang/zh-cn';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';
import { initializeUiTheme } from '/@/theme/themes';
import { NextLoading } from '/@/utils/loading';
import { Session } from '/@/utils/storage';

initializeUiTheme();
const app = createApp(App);

directive(app);
app.use(ElementPlus, { locale: zhCn });
app.use(pinia).use(router).use(i18n);
setupRouteFeatureGuards(app, router);

dayjs.extend(utc);
dayjs.extend(timezone);
app.config.globalProperties.mittBus = mitt();
app.config.globalProperties.$day = dayjs; //鍏ㄥ眬鎸傝浇

const bootstrap = async () => {
	try {
		await router.isReady();
		await ensureRouteFeatureInstall(app, router.currentRoute.value);
	} catch (error) {
		// 最终兜底：任何未被路由守卫捕获的启动异常都不能阻止应用挂载。
		console.error('[bootstrap] 应用初始化失败', error);
		Session.clear();
	} finally {
		app.mount('#app');
		NextLoading.done();
	}
};

void bootstrap();
