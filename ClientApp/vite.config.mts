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

const vendorChunkGroups: Array<[string, string[]]> = [
	['vue-vendor', ['/node_modules/vue/', '/node_modules/vue-router/', '/node_modules/pinia/', '/node_modules/vue-i18n/', '/node_modules/@vue/', '/node_modules/@intlify/', '/node_modules/vue-demi/']],
	['element-plus', ['/node_modules/element-plus/', '/node_modules/@element-plus/', '/node_modules/lodash-unified/']],
	['fast-crud', ['/node_modules/@fast-crud/']],
	['form-create', ['/node_modules/@form-create/']],
	['echarts-vendor', ['/node_modules/echarts/', '/node_modules/zrender/', '/node_modules/echarts-gl/', '/node_modules/echarts-wordcloud/']],
	['three-vendor', ['/node_modules/three/']],
	['antv-vendor', ['/node_modules/@antv/']],
	['diagram-vendor', ['/node_modules/jsplumb/']],
	['grid-layout', ['/node_modules/vue-grid-layout/']],
];

export default defineConfig(({ mode, command }: ConfigEnv) => {
	const env = loadEnv(mode, process.cwd());
	const apiProxyTarget = env.VITE_API_PROXY_TARGET || 'http://localhost:5000';

	return {
		plugins: [
			vue(),
			asPluginFactory(vueSetupExtend)(),
			asPluginFactory(viteCompression)({
				verbose: false,
			}),
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
			// Monaco language workers are isolated into dedicated chunks and remain naturally large.
			chunkSizeWarningLimit: 5000,
			rollupOptions: {
				output: {
					chunkFileNames: 'assets/js/[name]-[hash].js',
					entryFileNames: 'assets/js/[name]-[hash].js',
					assetFileNames: 'assets/[ext]/[name]-[hash].[ext]',
					manualChunks(id) {
						const normalizedId = id.replace(/\\/g, '/');
						if (normalizedId.includes('vite/preload-helper')) return 'vite-preload';
						if (normalizedId.includes('commonjsHelpers.js')) return 'rollup-helpers';
						if (normalizedId.includes('/node_modules/')) {
							// Monaco is loaded through dynamic imports and splits more safely when Rollup
							// controls its internal graph instead of hard manual chunk boundaries.
							if (normalizedId.includes('/node_modules/monaco-editor/')) return;
							for (const [chunkName, patterns] of vendorChunkGroups) {
								if (patterns.some((pattern) => normalizedId.includes(pattern))) return chunkName;
							}
							return normalizedId.match(/\/node_modules\/(?!.pnpm)(?<moduleName>[^\/]*)\//)?.groups?.moduleName ?? 'vendor';
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
