import path from 'node:path';
import { fileURLToPath } from 'node:url';
import vue from '@vitejs/plugin-vue';
import { defineConfig, loadEnv, type ConfigEnv } from 'vite';
import vueSetupExtend from 'vite-plugin-vue-setup-extend-plus';
import viteCompression from 'vite-plugin-compression';
import { buildConfig } from './src/utils/build';
import AutoImport from 'unplugin-auto-import/vite';
import Icons from 'unplugin-icons/vite';
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers';
import Components from 'unplugin-vue-components/vite';
import IconsResolver from 'unplugin-icons/resolver';

const asPluginFactory = <T>(plugin: T) => {
	let current: any = plugin;
	while (current && typeof current !== 'function' && 'default' in current) {
		current = current.default;
	}
	return current as T;
};
const currentDir = path.dirname(fileURLToPath(import.meta.url));
const pathResolve = (dir: string) => path.resolve(currentDir, dir);
const pathSrc = pathResolve('src');

const alias: Record<string, string> = {
	'/@': pathResolve('src'),
	'vue-i18n': 'vue-i18n/dist/vue-i18n.cjs.js',
};

const createBackendProxy = (target: string) => ({
	target,
	ws: true,
	changeOrigin: true,
});

export default defineConfig(({ mode, command }: ConfigEnv) => {
	const env = loadEnv(mode, process.cwd());
	const apiProxyTarget = env.VITE_API_PROXY_TARGET || 'http://localhost:5000';

	return {
		plugins: [
			vue(),
			asPluginFactory(vueSetupExtend)(),
			asPluginFactory(viteCompression)(),
			JSON.parse(env.VITE_OPEN_CDN) ? buildConfig.cdn() : null,
			AutoImport({
				imports: ['vue', 'vue-router', 'pinia'],
				dirs: ['./stores'],
				eslintrc: {
					enabled: true,
					filepath: './.eslintrc-auto-import.json',
					globalsPropValue: true,
				},
				resolvers: [
					ElementPlusResolver(),
					IconsResolver({
						prefix: 'Icon',
					}),
				],
				dts: path.resolve(pathSrc, 'auto-imports.d.ts'),
			}),
			Components({
				resolvers: [
					IconsResolver({
						prefix: 'icon',
					}),
					ElementPlusResolver(),
				],
				dts: path.resolve(pathSrc, 'components.d.ts'),
			}),
			Icons({
				compiler: 'vue3',
				autoInstall: true,
			}),
		],
		root: process.cwd(),
		resolve: { alias },
		base: command === 'serve' ? './' : env.VITE_PUBLIC_PATH,
		optimizeDeps: {
			exclude: ['vue-demi'],
			entries: ['src/**/*.vue', 'src/**/*.ts'],
		},
		server: {
			host: '0.0.0.0',
			port: env.VITE_PORT as unknown as number,
			open: JSON.parse(env.VITE_OPEN),
			hmr: true,
			proxy: {
				'/api': createBackendProxy(apiProxyTarget),
				'/healthz': createBackendProxy(apiProxyTarget),
				'/gitee': {
					target: 'https://gitee.com',
					ws: true,
					changeOrigin: true,
					rewrite: (requestPath) => requestPath.replace(/^\/gitee/, ''),
				},
				'/models/gltf': {
					...createBackendProxy(apiProxyTarget),
					rewrite: (requestPath) => requestPath.replace(/^\/gitee/, ''),
				},
				'/textures': {
					...createBackendProxy(apiProxyTarget),
					rewrite: (requestPath) => requestPath.replace(/^\/gitee/, ''),
				},
			},
		},
		build: {
			outDir: 'dist',
			chunkSizeWarningLimit: 1500,
			rollupOptions: {
				output: {
					chunkFileNames: 'assets/js/[name]-[hash].js',
					entryFileNames: 'assets/js/[name]-[hash].js',
					assetFileNames: 'assets/[ext]/[name]-[hash].[ext]',
					manualChunks(id) {
						if (id.includes('node_modules')) {
							return id.toString().match(/\/node_modules\/(?!.pnpm)(?<moduleName>[^\/]*)\//)?.groups!.moduleName ?? 'vender';
						}
					},
				},
				...(JSON.parse(env.VITE_OPEN_CDN) ? { external: buildConfig.external } : {}),
			},
		},
		css: {
			preprocessorOptions: {
				css: { charset: false },
				sass: { api: 'modern' },
				scss: { api: 'modern' },
			},
		},
		define: {
			__VUE_I18N_LEGACY_API__: JSON.stringify(false),
			__VUE_I18N_FULL_INSTALL__: JSON.stringify(false),
			__INTLIFY_PROD_DEVTOOLS__: JSON.stringify(false),
			__IOTSHARP_VERSION__: JSON.stringify(process.env.npm_package_version),
			__IOTSHARP_APP_ID__: JSON.stringify(process.env.npm_package_name),
		},
	};
});
