<template>
	<div>
		<el-drawer
			v-model="state.drawer"
			:size="'82%'"
			class="produce-form-drawer"
			append-to-body
			destroy-on-close
		>
			<ConsoleDrawerWorkspace
				:eyebrow="isEditMode ? 'Product Edit' : 'Product Setup'"
				:title="state.dialogtitle"
				:description="drawerDescription"
				:badges="badges"
				:metrics="metrics"
			>
				<template #actions>
					<el-button @click="closeDialog">取消</el-button>
					<el-button type="primary" :loading="submitting" @click="onSubmit(dataFormRef)">保存产品</el-button>
				</template>

				<el-form ref="dataFormRef" :model="state.dataForm" :rules="rules" label-position="top" class="produce-form">
					<section class="produce-form__grid">
						<article class="form-card">
							<div class="form-card__header">
								<div>
									<h3>产品资料</h3>
									<p>先确定产品名称、图标和业务说明，方便后续设备建模、运营协同和列表检索。</p>
								</div>
							</div>
							<el-row :gutter="18">
								<el-col :xs="24" :md="14">
									<el-form-item label="产品名称" prop="name">
										<el-input v-model="state.dataForm.name" placeholder="请输入产品名称" clearable />
									</el-form-item>
								</el-col>
								<el-col :xs="24" :md="10">
									<el-form-item label="产品图标" prop="icon">
										<IconSelector v-model="state.dataForm.icon" />
									</el-form-item>
								</el-col>
								<el-col :span="24">
									<el-form-item label="产品说明" prop="description">
										<el-input
											v-model="state.dataForm.description"
											type="textarea"
											:rows="5"
											placeholder="补充产品用途、行业场景或接入特点，方便团队协同。"
											clearable
										/>
									</el-form-item>
								</el-col>
							</el-row>
						</article>

						<article class="form-card">
							<div class="form-card__header">
								<div>
									<h3>默认接入策略</h3>
									<p>这些配置会成为后续设备创建时的默认模板，建议在这里统一收敛认证和网关接入策略。</p>
								</div>
							</div>
							<el-row :gutter="18">
								<el-col :xs="24" :md="12">
									<el-form-item label="默认网关类型" prop="gatewayType">
										<el-select v-model="state.dataForm.gatewayType" placeholder="请选择默认网关类型">
											<el-option
												v-for="item in gatewayTypeOptions"
												:key="item.value"
												:label="item.label"
												:value="item.value"
											/>
										</el-select>
									</el-form-item>
								</el-col>
								<el-col :xs="24" :md="12">
									<el-form-item label="默认设备类型" prop="defaultDeviceType">
										<el-select v-model="state.dataForm.defaultDeviceType" placeholder="请选择默认设备类型">
											<el-option
												v-for="item in deviceTypeOptions"
												:key="item.value"
												:label="item.label"
												:value="item.value"
											/>
										</el-select>
									</el-form-item>
								</el-col>
								<el-col :xs="24" :md="12">
									<el-form-item label="认证方式" prop="defaultIdentityType">
										<el-select v-model="state.dataForm.defaultIdentityType" placeholder="请选择认证方式">
											<el-option
												v-for="item in identityTypeOptions"
												:key="item.value"
												:label="item.label"
												:value="item.value"
											/>
										</el-select>
									</el-form-item>
								</el-col>
								<el-col :xs="24" :md="12">
									<el-form-item label="默认超时（秒）" prop="defaultTimeout">
										<el-input-number v-model="state.dataForm.defaultTimeout" :min="1" :max="86400" controls-position="right" />
									</el-form-item>
								</el-col>
							</el-row>
						</article>

						<article class="form-card form-card--full">
							<div class="form-card__header">
								<div>
									<h3>网关配置工作区</h3>
									<p>{{ gatewayConfigurationDescription }}</p>
								</div>
								<div class="form-card__badge">{{ gatewayTypeLabel }}</div>
							</div>

							<div v-if="showNamedGatewayConfig" class="gateway-config gateway-config--compact">
								<el-form-item label="网关配置名称">
									<el-input
										v-model="state.dataForm.gatewayConfigurationName"
										placeholder="请输入默认网关配置名称"
										clearable
									/>
								</el-form-item>
							</div>

							<div v-else-if="showCustomGatewayConfig" class="gateway-config">
								<el-form-item label="自定义网关配置（JSON）">
									<monaco
										height="360px"
										width="100%"
										theme="vs-dark"
										v-model="state.dataForm.gatewayConfigurationJson"
										language="json"
										selectOnLineNumbers="true"
										@change="oneditorchange"
									/>
								</el-form-item>
							</div>

							<div v-else class="gateway-placeholder">
								<strong>当前无需额外网关配置</strong>
								<p>如果后续需要自定义连接器或指定具体配置模板，直接切换默认网关类型即可继续补充。</p>
							</div>
						</article>
					</section>
				</el-form>
			</ConsoleDrawerWorkspace>
		</el-drawer>
	</div>
</template>

<script lang="ts" setup>
import { computed, nextTick, reactive, ref, watch } from 'vue';
import { ElMessage, type FormInstance, type FormRules } from 'element-plus';
import { NIL as NIL_UUID } from 'uuid';
import type { produceaddoreditdto } from '/@/dtos/produceaddoreditdto';
import { getProduce, saveProduce, updateProduce } from '/@/api/produce';
import ConsoleDrawerWorkspace from '/@/components/console/ConsoleDrawerWorkspace.vue';
import monaco from '/@/components/monaco/monaco.vue';
import IconSelector from '/@/components/iconSelector/index.vue';

interface ProduceFormState {
	drawer: boolean;
	dialogtitle: string;
	dataForm: produceaddoreditdto;
}

const gatewayTypeOptions = [
	{ value: 'Unknow', label: '未指定' },
	{ value: 'Customize', label: '自定义接入' },
	{ value: 'Modbus', label: 'Modbus' },
	{ value: 'Bacnet', label: 'Bacnet' },
	{ value: 'OPC_UA', label: 'OPC UA' },
	{ value: 'CanOpen', label: 'CanOpen' },
];

const identityTypeOptions = [
	{ value: 'AccessToken', label: 'AccessToken' },
	{ value: 'DevicePassword', label: '设备密码' },
	{ value: 'X509Certificate', label: 'X509 证书' },
];

const deviceTypeOptions = [
	{ value: 'Device', label: '设备' },
	{ value: 'Gateway', label: '网关' },
];

const emit = defineEmits(['close', 'submit']);
const dataFormRef = ref<FormInstance>();
const submitting = ref(false);

const createDefaultForm = (): produceaddoreditdto => ({
	id: NIL_UUID,
	name: '',
	icon: '',
	defaultTimeout: 300,
	gatewayType: 'Unknow',
	description: '',
	gatewayConfigurationName: '',
	defaultDeviceType: 'Device',
	gatewayConfigurationJson: '',
	gatewayConfiguration: '',
	defaultIdentityType: 'AccessToken',
});

const rules = reactive<FormRules>({
	name: [
		{ required: true, message: '请输入产品名称', trigger: 'blur' },
		{ min: 2, message: '产品名称至少 2 个字符', trigger: 'blur' },
	],
	gatewayType: [{ required: true, message: '请选择默认网关类型', trigger: 'change' }],
	defaultDeviceType: [{ required: true, message: '请选择默认设备类型', trigger: 'change' }],
	defaultIdentityType: [{ required: true, message: '请选择认证方式', trigger: 'change' }],
	defaultTimeout: [{ required: true, message: '请输入默认超时', trigger: 'change' }],
});

const state = reactive<ProduceFormState>({
	drawer: false,
	dialogtitle: '',
	dataForm: createDefaultForm(),
});

const isEditMode = computed(() => state.dataForm.id && state.dataForm.id !== NIL_UUID);
const gatewayTypeLabel = computed(() => {
	return gatewayTypeOptions.find((item) => item.value === state.dataForm.gatewayType)?.label || '未指定';
});
const drawerDescription = computed(() => {
	return isEditMode.value
		? '继续在统一的产品建模工作台里维护基础资料、默认接入策略和网关配置。'
		: '在这里创建新的产品模型，为后续设备创建、属性字典和映射设计打下统一基础。';
});
const showCustomGatewayConfig = computed(() => state.dataForm.gatewayType === 'Customize');
const showNamedGatewayConfig = computed(() => {
	return !!state.dataForm.gatewayType && state.dataForm.gatewayType !== 'Unknow' && state.dataForm.gatewayType !== 'Customize';
});
const gatewayConfigurationDescription = computed(() => {
	if (showCustomGatewayConfig.value) {
		return '当前已切换为自定义接入模式，可以直接编写 JSON 网关配置。';
	}
	if (showNamedGatewayConfig.value) {
		return '当前接入方式依赖命名配置模板，适合绑定已有网关配置。';
	}
	return '尚未指定具体接入方式时，无需额外补充网关配置。';
});

const badges = computed(() => [
	isEditMode.value ? '编辑模式' : '新建模式',
	gatewayTypeLabel.value,
	state.dataForm.name || '未命名产品',
]);

const metrics = computed(() => [
	{
		label: '默认设备类型',
		value: deviceTypeOptions.find((item) => item.value === state.dataForm.defaultDeviceType)?.label || '--',
		hint: '后续从产品创建的设备会继承这一默认类型。',
		tone: 'primary' as const,
	},
	{
		label: '认证方式',
		value: identityTypeOptions.find((item) => item.value === state.dataForm.defaultIdentityType)?.label || '--',
		hint: '保持统一认证方式可以减少设备接入碎片化。',
		tone: 'accent' as const,
	},
	{
		label: '网关模式',
		value: gatewayTypeLabel.value,
		hint: '支持未指定、命名配置和自定义 JSON 三种节奏。',
		tone: 'success' as const,
	},
	{
		label: '默认超时',
		value: `${state.dataForm.defaultTimeout ?? '--'} 秒`,
		hint: '作为设备通信和控制的默认超时基线。',
		tone: 'warning' as const,
	},
]);

watch(
	() => state.dataForm.gatewayType,
	(value) => {
		if (value === 'Customize') {
			state.dataForm.gatewayConfigurationName = '';
			return;
		}

		state.dataForm.gatewayConfigurationJson = '';
		if (value === 'Unknow') {
			state.dataForm.gatewayConfigurationName = '';
		}
	}
);

const resetFormState = () => {
	state.dataForm = createDefaultForm();
};

const fillGatewayConfigurationFields = (rawProduce: any) => {
	const form = createDefaultForm();
	form.id = rawProduce.id;
	form.name = rawProduce.name;
	form.icon = rawProduce.icon;
	form.defaultTimeout = rawProduce.defaultTimeout;
	form.description = rawProduce.description;
	form.gatewayType = rawProduce.gatewayType || 'Unknow';
	form.defaultDeviceType = rawProduce.defaultDeviceType;
	form.defaultIdentityType = rawProduce.defaultIdentityType;

	if (form.gatewayType === 'Customize') {
		form.gatewayConfigurationJson = rawProduce.gatewayConfiguration;
	}
	else if (form.gatewayType !== 'Unknow') {
		form.gatewayConfigurationName = rawProduce.gatewayConfiguration;
	}

	state.dataForm = form;
};

const openDialog = async (produceid: string) => {
	state.drawer = true;

	if (produceid === NIL_UUID) {
		resetFormState();
		state.dialogtitle = '新建产品';
		await nextTick();
		dataFormRef.value?.clearValidate();
		return;
	}

	state.dialogtitle = '编辑产品';
	const res = await getProduce(produceid);
	fillGatewayConfigurationFields(res.data);
	await nextTick();
	dataFormRef.value?.clearValidate();
};

const closeDialog = () => {
	state.drawer = false;
};

const oneditorchange = () => {
	return undefined;
};

const applyGatewayConfiguration = () => {
	if (showCustomGatewayConfig.value) {
		state.dataForm.gatewayConfiguration = state.dataForm.gatewayConfigurationJson?.trim() ?? '';
		return;
	}

	if (showNamedGatewayConfig.value) {
		state.dataForm.gatewayConfiguration = state.dataForm.gatewayConfigurationName?.trim() ?? '';
		return;
	}

	state.dataForm.gatewayConfiguration = '';
};

const validateGatewayConfiguration = () => {
	if (showCustomGatewayConfig.value && !state.dataForm.gatewayConfigurationJson?.trim()) {
		ElMessage.warning('请填写自定义网关配置 JSON');
		return false;
	}

	if (showNamedGatewayConfig.value && !state.dataForm.gatewayConfigurationName?.trim()) {
		ElMessage.warning('请填写默认网关配置名称');
		return false;
	}

	return true;
};

const onSubmit = async (formEl: FormInstance | undefined) => {
	if (!formEl || submitting.value) return;

	await formEl.validate(async (valid) => {
		if (!valid || !validateGatewayConfiguration()) return;

		submitting.value = true;
		applyGatewayConfiguration();

		try {
			if (state.dataForm.id === NIL_UUID) {
				const result = await saveProduce(state.dataForm);
				if (result.code === 10000) {
					ElMessage.success('产品创建成功');
					emit('close', state.dataForm);
					state.drawer = false;
				}
				else {
					ElMessage.warning(`产品创建失败: ${result.msg}`);
				}
			}
			else {
				const result = await updateProduce(state.dataForm);
				if (result.code === 10000) {
					ElMessage.success('产品更新成功');
					emit('close', state.dataForm);
					state.drawer = false;
				}
				else {
					ElMessage.warning(`产品更新失败: ${result.msg}`);
				}
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
:deep(.produce-form-drawer .el-drawer__header) {
	margin-bottom: 0;
	padding-bottom: 0;
}

:deep(.produce-form-drawer .el-drawer__body) {
	padding: 18px;
	background: linear-gradient(180deg, #f8fbff 0%, #f3f7fc 100%);
}

.produce-form__grid {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
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

.form-card--full {
	grid-column: 1 / -1;
}

.form-card__header {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 16px;
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

.form-card__badge {
	display: inline-flex;
	align-items: center;
	height: 34px;
	padding: 0 12px;
	border-radius: 999px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(219, 234, 254, 0.78);
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	white-space: nowrap;
}

.gateway-config,
.gateway-placeholder {
	padding: 18px;
	border-radius: 20px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: rgba(255, 255, 255, 0.78);
}

.gateway-config--compact {
	max-width: 560px;
}

.gateway-placeholder strong {
	display: block;
	color: #123b6d;
	font-size: 18px;
}

.gateway-placeholder p {
	margin: 10px 0 0;
	color: #64748b;
	font-size: 13px;
	line-height: 1.8;
}

.produce-form :deep(.el-form-item) {
	margin-bottom: 18px;
}

.produce-form :deep(.el-input__wrapper),
.produce-form :deep(.el-textarea__inner),
.produce-form :deep(.el-select__wrapper) {
	border-radius: 16px;
}

.produce-form :deep(.el-input-number) {
	width: 100%;
}

@media (max-width: 1080px) {
	.produce-form__grid {
		grid-template-columns: 1fr;
	}

	.form-card--full {
		grid-column: auto;
	}
}

@media (max-width: 767px) {
	:deep(.produce-form-drawer) {
		width: 100% !important;
	}

	:deep(.produce-form-drawer .el-drawer__body) {
		padding: 12px;
	}

	.form-card {
		padding: 16px;
		border-radius: 20px;
	}

	.form-card__header {
		flex-direction: column;
	}
}
</style>
