/**
 * 打包相关
 * 内网部署禁止启用远程 CDN，所有依赖都进入本地构建产物。
 */
export const buildConfig = {
	cdn() {
		return null;
	},
	external: [],
};
