import { RouteRecordRaw } from 'vue-router';

/**
 * 路由meta对象参数说明
 * meta: {
 *      title:          菜单栏及 tagsView 栏、菜单搜索名称（国际化）
 *      isLink：        是否超链接菜单，开启外链条件，`1、isLink: 链接地址不为空`
 *      isHide：        是否隐藏此路由
 *      isKeepAlive：   是否缓存组件状态
 *      isAffix：       是否固定在 tagsView 栏上
 *      isIframe：      是否内嵌窗口，开启条件，`1、isIframe:true 2、isLink：链接地址不为空`
 *      roles：         当前路由权限标识，取角色管理。控制路由显示、隐藏。超级管理员：admin 普通角色：common
 *      icon：          菜单、tagsView 图标，阿里：加 `iconfont xxx`，fontawesome：加 `fa xxx`
 * }
 */

/**
 * 定义动态路由
 * 前端添加路由，请在顶级节点的 `children 数组` 里添加
 * @description 未开启 isRequestRoutes 为 true 时使用（前端控制路由），开启时第一个顶级 children 的路由将被替换成接口请求回来的路由数据
 * @description 各字段请查看 `/@/views/system/menu/component/addMenu.vue 下的 ruleForm`
 * @returns 返回路由菜单数据
 */
export const dynamicRoutes: Array<RouteRecordRaw> = [
	{
		path: '/',
		name: '/',
		component: () => import('/@/layout/index.vue'),
		redirect: '/dashboard/v1',
		meta: {
			isKeepAlive: true,
		},
		children: [
			{
				path: '/dashboard/v1',
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

// 部分前端管理的路由
export const frontEndRoutes = [
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
		path: '/iot/gateway/gatewaydesigner',
		name: 'gatewaydesigner',
		component: () => import('/@/views/iot/gateway/gatewaydesigner.vue'),
		meta: {
			title: 'message.router.home',
			isHide: true,
		},
	},
]

/**
 * 定义404、401界面
 * @link 参考：https://next.router.vuejs.org/zh/guide/essentials/history-mode.html#netlify
 */
export const notFoundAndNoPower = [
	{
		path: '/:path(.*)*',
		name: 'notFound',
		component: () => import('/@/views/error/404.vue'),
		meta: {
			title: 'message.staticRoutes.notFound',
			isHide: true,
		},
	},
	{
		path: '/401',
		name: 'noPower',
		component: () => import('/@/views/error/401.vue'),
		meta: {
			title: 'message.staticRoutes.noPower',
			isHide: true,
		},
	},


];

/**
 * 定义静态路由（默认路由）
 * 此路由不要动，前端添加路由的话，请在 `dynamicRoutes 数组` 中添加
 * @description 前端控制直接改 dynamicRoutes 中的路由，后端控制不需要修改，请求接口路由数据时，会覆盖 dynamicRoutes 第一个顶级 children 的内容（全屏，不包含 layout 中的路由出口）
 * @returns 返回路由菜单数据
 */
export const staticRoutes: Array<RouteRecordRaw> = [
	{
		path: '/login',
		name: 'login',
		component: () => import('/@/views/login/index.vue'),
		meta: {
			title: '登录',
		},
	},
	{
		path: '/setup',
		name: 'setup',
		component: () => import('/@/views/setup/index.vue'),
		meta: {
			title: '初始化系统',
		},
	},
	{
		path: '/signup',
		name: 'signup',
		component: () => import('/@/views/login/signup.vue'),
		meta: {
			title: '初始化系统',
		},
	},
	/**
	 * 提示：写在这里的为全屏界面，不建议写在这里
	 * 请写在 `dynamicRoutes` 路由数组中
	 */

];
