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
			<div class="loading-next__panel" role="status" aria-live="polite" aria-label="IoTSharp loading">
				<div class="loading-next__badge">IoT Platform Control Plane</div>
				<div class="loading-next__visual" aria-hidden="true">
					<span class="loading-next__ring loading-next__ring--outer"></span>
					<span class="loading-next__ring loading-next__ring--middle"></span>
					<span class="loading-next__ring loading-next__ring--inner"></span>
					<span class="loading-next__beam"></span>
					<span class="loading-next__pulse"></span>
					<span class="loading-next__core"></span>
				</div>
				<div class="loading-next__text">
					<div class="loading-next__brand">IoTSharp</div>
					<div class="loading-next__title">Launching your device operations workspace</div>
					<div class="loading-next__meta">
						<span>Secure session handshake</span>
						<span>Device topology syncing</span>
						<span>Console modules loading</span>
					</div>
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
