<template>
	<div class="login-container">
		<div class="login-content">
			<div class="login-content-main">
        <div class="flex justify-center pt-40px pb-10px">
          <AppLogo></AppLogo>
        </div>
        <div class="flex justify-center text-gray-600 pb-30px text-16px pt-8px">Log in to your IoT Sharp account</div>
							<Account />
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import { toRefs, reactive, computed, defineComponent, onMounted } from 'vue';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import { NextLoading } from '/@/utils/loading';
import Account from '/@/views/login/component/account.vue';
import AppLogo from "/@/components/AppLogo.vue";


// 定义接口来定义对象的类型
interface LoginState {
	tabsActiveName: string;

}

export default defineComponent({
	name: 'loginIndex',
	components: {AppLogo, Account,},
	setup() {
		const storesThemeConfig = useThemeConfig();
		const { themeConfig } = storeToRefs(storesThemeConfig);
		const state = reactive<LoginState>({
			tabsActiveName: 'account',

		});
		// 获取布局配置信息
		const getThemeConfig = computed(() => {
			return themeConfig.value;
		});
		// 页面加载时
		onMounted(() => {
			NextLoading.done();
		});
		return {
			getThemeConfig,
			...toRefs(state),
		};
	},
});
</script>

<style scoped lang="scss">
.login-container {
	width: 100%;
	height: 100%;
	position: relative;
	background: var(--z-page-color);
	.login-icon-group {
		width: 100%;
		height: 100%;
		position: relative;
		.login-icon-group-title {
			position: absolute;
			top: 50px;
			left: 80px;
			display: flex;
			align-items: center;
			img {
				width: 30px;
				height: 30px;
			}
			&-text {
				padding-left: 15px;
				color: var(--el-color-primary);
			}
		}
		&-icon {
			width: 60%;
			height: 70%;
			position: absolute;
			left: 0;
			bottom: 0;
		}
	}
	.login-content {
		width: 500px;
		padding: 20px;
		position: absolute;
		left: calc(50% - 250px);
		top: 40%;
		transform: translateY(-50%) translate3d(0, 0, 0);
		background-color: var(--el-color-white);
    box-shadow: 0 1px 4px 0 rgba(33,33,52,0.10);
    border-radius: 6px;
		overflow: hidden;
		z-index: 1;
    padding-bottom: 60px;
		//height: 460px;
		.login-content-main {
			margin: 0 auto;
			width: 80%;
			.login-content-title {
				color: var(--el-text-color-primary);
				font-weight: 500;
				font-size: 22px;
				text-align: center;
				letter-spacing: 4px;
				//margin: 15px 0 30px;
				white-space: nowrap;
				z-index: 5;
				position: relative;
				transition: all 0.3s ease;
			}
		}
		.login-content-main-sacn {
			position: absolute;
			top: 0;
			right: 0;
			width: 50px;
			height: 50px;
			overflow: hidden;
			cursor: pointer;
			transition: all ease 0.3s;
			color: var(--el-text-color-primary);
			&-delta {
				position: absolute;
				width: 35px;
				height: 70px;
				z-index: 2;
				top: 2px;
				right: 21px;
				background: var(--el-color-white);
				transform: rotate(-45deg);
			}
			&:hover {
				opacity: 1;
				transition: all ease 0.3s;
				color: var(--el-color-primary) !important;
			}
			i {
				width: 47px;
				height: 50px;
				display: inline-block;
				font-size: 48px;
				position: absolute;
				right: 2px;
				top: -1px;
			}
		}
	}
}

</style>
