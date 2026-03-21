<template>
	<div>
		<el-drawer
			v-model="state.drawer"
			size="68%"
			class="produce-device-drawer"
			append-to-body
			destroy-on-close
		>
			<ConsoleDrawerWorkspace
				eyebrow="Create Device"
				:title="state.dialogtitle"
				description="基于当前产品模型快速创建设备，默认会继承产品层已经定义好的接入策略和建模约束。"
				:badges="badges"
				:metrics="metrics"
			>
				<template #actions>
					<el-button @click="closeDialog">取消</el-button>
					<el-button type="primary" :loading="submitting" @click="onSubmit(dataFormRef)">创建设备</el-button>
				</template>

				<section class="produce-device-layout">
					<article class="form-card form-card--main">
						<div class="form-card__header">
							<div>
								<h3>设备资料</h3>
								<p>只需填写设备名称即可快速落地，后续再到设备详情页补充更多运行时配置。</p>
							</div>
						</div>

						<el-form ref="dataFormRef" :model="state.dataForm" :rules="rules" label-position="top">
							<el-form-item label="设备名称" prop="name">
								<el-input v-model="state.dataForm.name" placeholder="请输入设备名称" clearable />
							</el-form-item>
						</el-form>
					</article>

					<aside class="form-card">
						<div class="form-card__header">
							<div>
								<h3>创建说明</h3>
								<p>这里创建的是基于产品模板的设备入口，适合在建模完成后快速批量落设备。</p>
							</div>
						</div>
						<ul class="device-tips">
							<li>设备会继承当前产品的默认认证方式和接入策略。</li>
							<li>如果需要更细的配置，可以创建设备后再进入设备详情页完善。</li>
							<li>建议保持设备名称可识别，方便后续映射和运营管理。</li>
						</ul>
					</aside>
				</section>
			</ConsoleDrawerWorkspace>
		</el-drawer>
	</div>
</template>

<script lang="ts" setup>
import { computed, nextTick, reactive, ref } from 'vue';
import { ElMessage, type FormInstance, type FormRules } from 'element-plus';
import { createDevice } from '/@/api/produce';
import ConsoleDrawerWorkspace from '/@/components/console/ConsoleDrawerWorkspace.vue';

interface DeviceForm {
	name: string;
}

interface ProduceDeviceState {
	drawer: boolean;
	dialogtitle: string;
	dataForm: DeviceForm;
	produceid: string;
}

const emit = defineEmits(['close', 'submit']);
const dataFormRef = ref<FormInstance>();
const submitting = ref(false);

const rules = reactive<FormRules>({
	name: [
		{ required: true, message: '请输入设备名称', trigger: 'blur' },
		{ min: 2, message: '设备名称至少 2 个字符', trigger: 'blur' },
	],
});

const state = reactive<ProduceDeviceState>({
	drawer: false,
	dialogtitle: '创建设备',
	dataForm: {
		name: '',
	},
	produceid: '',
});

const shortProduceId = computed(() => {
	if (!state.produceid) return '--';
	return `${state.produceid.slice(0, 12)}...`;
});

const badges = computed(() => [
	'产品派生设备',
	state.dataForm.name || '待命名设备',
]);

const metrics = computed(() => [
	{
		label: '来源产品',
		value: shortProduceId.value,
		hint: '设备将基于这个产品模型创建。',
		tone: 'primary' as const,
	},
	{
		label: '命名状态',
		value: state.dataForm.name ? '已填写' : '待填写',
		hint: '设备名称是当前步骤唯一必填项。',
		tone: 'accent' as const,
	},
	{
		label: '创建节奏',
		value: '快速创建',
		hint: '适合建模完成后的快速落设备场景。',
		tone: 'success' as const,
	},
	{
		label: '后续入口',
		value: '设备详情',
		hint: '创建后可继续到设备详情页补充配置。',
		tone: 'warning' as const,
	},
]);

const openDialog = async (produceid: string) => {
	state.produceid = produceid;
	state.dataForm = { name: '' };
	state.drawer = true;
	await nextTick();
	dataFormRef.value?.clearValidate();
};

const closeDialog = () => {
	state.drawer = false;
	emit('close', { sender: '', args: state.dataForm });
};

const onSubmit = async (formEl: FormInstance | undefined) => {
	if (!formEl || submitting.value) return;

	await formEl.validate(async (valid) => {
		if (!valid) return;

		submitting.value = true;
		try {
			const result = await createDevice(state.produceid, state.dataForm);
			if (result.code === 10000) {
				ElMessage.success('设备创建成功');
				state.drawer = false;
				emit('close', { sender: 'deviceform', args: state.dataForm });
			}
			else {
				ElMessage.warning(`设备创建失败: ${result.msg}`);
				emit('close', state.dataForm);
			}
		}
		finally {
			submitting.value = false;
		}
	});
};

defineExpose({
	openDialog,
});
</script>

<style lang="scss" scoped>
:deep(.produce-device-drawer .el-drawer__header) {
	margin-bottom: 0;
	padding-bottom: 0;
}

:deep(.produce-device-drawer .el-drawer__body) {
	padding: 18px;
	background: linear-gradient(180deg, #f8fbff 0%, #f3f7fc 100%);
}

.produce-device-layout {
	display: grid;
	grid-template-columns: minmax(0, 1.4fr) 320px;
	gap: 18px;
}

.form-card {
	padding: 22px;
	border-radius: 24px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background:
		radial-gradient(circle at top right, rgba(59, 130, 246, 0.08), transparent 32%),
		linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.96));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.form-card__header {
	margin-bottom: 16px;

	h3 {
		margin: 0 0 8px;
		color: #123b6d;
		font-size: 20px;
		letter-spacing: -0.04em;
	}

	p {
		margin: 0;
		color: #64748b;
		font-size: 13px;
		line-height: 1.75;
	}
}

.form-card :deep(.el-input__wrapper) {
	border-radius: 16px;
}

.device-tips {
	margin: 0;
	padding-left: 18px;
	color: #475569;
	font-size: 13px;
	line-height: 1.8;
}

@media (max-width: 1080px) {
	.produce-device-layout {
		grid-template-columns: 1fr;
	}
}

@media (max-width: 767px) {
	:deep(.produce-device-drawer) {
		width: 100% !important;
	}

	:deep(.produce-device-drawer .el-drawer__body) {
		padding: 12px;
	}

	.form-card {
		padding: 16px;
		border-radius: 20px;
	}
}
</style>
