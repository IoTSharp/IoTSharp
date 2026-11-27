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
			<div class="loading-next__halo loading-next__halo--left"></div>
			<div class="loading-next__halo loading-next__halo--right"></div>
			<div class="loading-next__shell" role="status" aria-live="polite" aria-label="IoTSharp 正在加载">
				<header class="loading-next__header">
					<div class="loading-next__brandline">
						<span class="loading-next__mark" aria-hidden="true"><i></i><b></b></span>
						<span class="loading-next__brand-copy"><strong>IoTSharp</strong><small>控制平面</small></span>
					</div>
					<div class="loading-next__secure"><span></span>安全连接</div>
				</header>
				<div class="loading-next__body">
					<section class="loading-next__signal" aria-hidden="true">
						<div class="loading-next__eyebrow">PLATFORM BOOTSTRAP</div>
						<div class="loading-next__visual">
							<span class="loading-next__ring loading-next__ring--outer"></span>
							<span class="loading-next__ring loading-next__ring--middle"></span>
							<span class="loading-next__ring loading-next__ring--inner"></span>
							<span class="loading-next__beam"></span>
							<span class="loading-next__core"><b>IS</b><small>IoTSharp</small></span>
							<span class="loading-next__node loading-next__node--top">租户</span>
							<span class="loading-next__node loading-next__node--right">设备</span>
							<span class="loading-next__node loading-next__node--bottom">权限</span>
						</div>
						<div class="loading-next__signal-caption"><strong>把运行上下文接回工作区</strong><span>租户、设备与导航权限正在同步</span></div>
					</section>
					<section class="loading-next__status">
						<div class="loading-next__eyebrow">正在连接工作区</div>
						<h1>工作区即将就绪</h1>
						<p class="loading-next__subtitle">加载完成后，将继续进入你有权限访问的工作区。</p>
						<div class="loading-next__progress-head"><span>加载应用模块</span><strong>正在处理</strong></div>
						<div class="loading-next__progress" aria-hidden="true"><span></span></div>
						<ul class="loading-next__steps">
							<li class="is-active"><span class="loading-next__step-icon"></span><span>应用模块</span><small>正在连接</small></li>
							<li><span class="loading-next__step-icon"></span><span>导航权限</span><small>等待同步</small></li>
							<li><span class="loading-next__step-icon"></span><span>工作区状态</span><small>等待同步</small></li>
						</ul>
					</section>
				</div>
				<footer class="loading-next__footer" aria-hidden="true">
					<span>接入与采集</span><i></i><span>实时规则</span><i></i><span>运维与发布</span>
					<em>IoTSharp console</em>
				</footer>
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
