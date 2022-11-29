import path from 'path'
import vue from '@vitejs/plugin-vue';
import { resolve } from 'path';
import { defineConfig, loadEnv, ConfigEnv } from 'vite';
import WindiCSS from 'vite-plugin-windicss';
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers'
import Icons from 'unplugin-icons/vite'
import IconsResolver from 'unplugin-icons/resolver'
const pathResolve = (dir: string): any => {
	return resolve(__dirname, '.', dir);
};
const pathSrc = path.resolve(__dirname, 'src')
const alias: Record<string, string> = {
	'/@': pathResolve('./src/'),
	'vue-i18n': 'vue-i18n/dist/vue-i18n.cjs.js',
};

const viteConfig = defineConfig((mode: ConfigEnv) => {
	const env = loadEnv(mode.mode, process.cwd());
	return {
		plugins: [
			vue(),
			WindiCSS(),
			AutoImport({
				imports:['vue', 'vue-router', 'pinia'],
				dirs: ['./stores'],
				eslintrc: {
					enabled: true, // Default `false`
					filepath: './.eslintrc-auto-import.json', // Default `./.eslintrc-auto-import.json`
					globalsPropValue: true, // Default `true`, (true | false | 'readonly' | 'readable' | 'writable' | 'writeable')
				},
				resolvers: [
					ElementPlusResolver(),
					// Auto import icon components
					// 自动导入图标组件
					IconsResolver({
						prefix: 'Icon',
					}),
				],
				dts: path.resolve(pathSrc, 'auto-imports.d.ts'),
			}),
			Components({
				resolvers: [
					// Auto register icon components
					// 自动注册图标组件
					IconsResolver({
						prefix: 'icon',
					}),
					ElementPlusResolver()
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
		base: mode.command === 'serve' ? './' : env.VITE_PUBLIC_PATH,
		hmr: true,
		optimizeDeps: {
			include: [
				'element-plus/lib/locale/lang/zh-cn',
				'element-plus/lib/locale/lang/en',
				'element-plus/lib/locale/lang/zh-tw',
				'monaco-editor/esm/vs/language/json/json.worker',
				'monaco-editor/esm/vs/language/css/css.worker',
				'monaco-editor/esm/vs/language/html/html.worker',
				'monaco-editor/esm/vs/language/typescript/ts.worker',
				'monaco-editor/esm/vs/editor/editor.worker',
			],
		},
		server: {
			host: '0.0.0.0',
			port: env.VITE_PORT as unknown as number,
			open: env.VITE_OPEN,
		},
		esbuild: {
			drop: ['console', 'debugger'],
		},
		build: {
			rollupOptions: {
				output: {
					compact: true,
					manualChunks: {
						vue: ['vue', 'vue-router', 'pinia'],
						echarts: ['echarts'],
					},
				},
			},
		},
		css: { preprocessorOptions: { css: { charset: false } } },
		define: {
			__VUE_I18N_LEGACY_API__: JSON.stringify(false),
			__VUE_I18N_FULL_INSTALL__: JSON.stringify(false),
			__INTLIFY_PROD_DEVTOOLS__: JSON.stringify(false),
		},
	};
});

export default viteConfig;
