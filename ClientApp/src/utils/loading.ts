import { nextTick } from 'vue';
import '/@/theme/loading.scss';

/**
 * Page-level global loading overlay.
 * `start` mounts the animated shell and `done` removes it with a short fade.
 */
export const NextLoading = {
	// Create the branded loading overlay once.
	start: () => {
		if (document.querySelector('.loading-next')) return;

		const bodys: Element = document.body;
		const div = <HTMLElement>document.createElement('div');
		div.setAttribute('class', 'loading-next');
		const htmls = `
			<div class="loading-next__backdrop"></div>
			<div class="loading-next__grid"></div>
			<div class="loading-next__glow loading-next__glow--left"></div>
			<div class="loading-next__glow loading-next__glow--right"></div>
			<div class="loading-next__panel" role="status" aria-live="polite" aria-label="IoTSharp 正在加载">
				<div class="loading-next__brandline">
					<span class="loading-next__mark" aria-hidden="true">IS</span>
					<span class="loading-next__brand">IoTSharp</span>
					<span class="loading-next__badge">控制平面</span>
				</div>
				<div class="loading-next__body">
					<div class="loading-next__visual" aria-hidden="true">
						<span class="loading-next__ring loading-next__ring--outer"></span>
						<span class="loading-next__ring loading-next__ring--middle"></span>
						<span class="loading-next__ring loading-next__ring--inner"></span>
						<span class="loading-next__beam"></span>
						<span class="loading-next__pulse"></span>
						<span class="loading-next__core"></span>
					</div>
					<div class="loading-next__text">
						<div class="loading-next__eyebrow">正在连接工作区</div>
						<div class="loading-next__title">工作区即将就绪</div>
						<div class="loading-next__subtitle">正在加载租户、设备与导航权限</div>
						<div class="loading-next__progress" aria-hidden="true"><span></span></div>
						<div class="loading-next__status"><span class="loading-next__status-dot"></span><span>加载应用模块</span><span class="loading-next__status-line"></span><span>请稍候</span></div>
					</div>
				</div>
				<div class="loading-next__footer" aria-hidden="true">
					<span>接入与采集</span><i></i><span>实时规则</span><i></i><span>运维与发布</span>
				</div>
			</div>
		`;
		div.innerHTML = htmls;
		bodys.insertBefore(div, bodys.childNodes[0]);
		window.nextLoading = true;
	},
	// Remove the loading overlay after a brief transition.
	done: (time: number = 0) => {
		nextTick(() => {
			setTimeout(() => {
				const el = <HTMLElement>document.querySelector('.loading-next');
				if (!el) {
					window.nextLoading = false;
					return;
				}

				el.classList.add('loading-next--leave');
				window.setTimeout(() => {
					window.nextLoading = false;
					el.parentNode?.removeChild(el);
				}, 320);
			}, time);
		});
	},
};
