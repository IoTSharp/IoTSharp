import { RouteRecordRaw } from 'vue-router';

declare module 'vue-router' {
	interface RouteMeta {
		title?: string;
		isLink?: string;
		isHide?: boolean;
		isKeepAlive?: boolean;
		isAffix?: boolean;
		isIframe?: boolean;
		roles?: string[];
		icon?: string;
	}
}

export const dynamicRoutes: Array<RouteRecordRaw> = [
	{
		path: '/console',
		name: 'console-shell',
		component: () => import('/@/layout/index.vue'),
		redirect: '/dashboard',
		meta: {
			isKeepAlive: true,
		},
		children: [
			{
				path: '/dashboard',
				name: 'dashboard',
				component: () => import('/@/views/dashboard/index.vue'),
				meta: {
					title: 'message.router.home',
					isLink: '',
					isHide: false,
					isKeepAlive: true,
					isAffix: true,
					isIframe: false,
					roles: ['admin', 'common'],
					icon: 'iconfont icon-shouye',
				},
			},
		],
	},
];

export const frontEndRoutes: Array<RouteRecordRaw> = [
	{
		path: '/profile',
		name: 'profile',
		component: () => import('/@/views/profile/index.vue'),
		meta: {
			title: '个人中心',
			isHide: true,
		},
	},
	{
		path: '/iot/rules/flowdesigner',
		name: 'flowdesigner',
		component: () => import('/@/views/iot/rules/flowdesigner.vue'),
		meta: {
			title: '规则设计器',
			isHide: true,
		},
	},
	{
		path: '/iot/forms/edit',
		name: 'edit',
		component: () => import('/@/views/iot/forms/edit.vue'),
		meta: {
			title: '编辑',
			isHide: true,
		},
	},
	{
		path: '/iot/rules/flowsimulator',
		name: 'flowsimulator',
		component: () => import('/@/views/iot/rules/flowsimulator.vue'),
		meta: {
			title: 'message.router.home',
			isHide: true,
		},
	},
	{
		path: '/iot/rules/flowevents',
		name: 'flowevents',
		component: () => import('/@/views/iot/rules/flowevents.vue'),
		meta: {
			title: 'message.router.home',
			isHide: true,
		},
	},
	{
		path: '/iot/devices/assetdesigner',
		name: 'assetdesigner',
		component: () => import('/@/views/iot/assets/designer/assetdesigner.vue'),
		meta: {
			title: 'message.router.home',
			isHide: true,
		},
	},
	{
		path: '/iot/devices/gatewaydesigner',
		name: 'gatewaydesigner',
		component: () => import('/@/views/iot/devices/gatewaydesigner.vue'),
		meta: {
			title: 'message.router.home',
			isHide: true,
		},
	},
];

export const notFoundAndNoPower: Array<RouteRecordRaw> = [
	{
		path: '/401',
		name: 'noPower',
		component: () => import('/@/views/error/401.vue'),
		meta: {
			title: 'message.staticRoutes.noPower',
			isHide: true,
		},
	},
	{
		path: '/:path(.*)*',
		name: 'notFound',
		component: () => import('/@/views/error/404.vue'),
		meta: {
			title: 'message.staticRoutes.notFound',
			isHide: true,
		},
	},
];

export const staticRoutes: Array<RouteRecordRaw> = [
	{
		path: '/',
		name: 'landing',
		component: () => import('/@/views/landing/index.vue'),
		meta: {
			title: 'IoTSharp',
		},
	},
	{
		path: '/login',
		name: 'login',
		component: () => import('/@/views/login/index.vue'),
		meta: {
			title: '登录',
		},
	},
	{
		path: '/signup',
		name: 'signup',
		component: () => import('/@/views/login/signup.vue'),
		meta: {
			title: '注册',
		},
	},
	{
		path: '/installer',
		name: 'installer',
		component: () => import('/@/views/installer/index.vue'),
		meta: {
			title: '初始化系统',
		},
	},
];
