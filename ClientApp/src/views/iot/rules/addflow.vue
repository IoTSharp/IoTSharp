<template>
	<div>
		<el-drawer
			v-model="state.drawer"
			size="72%"
			class="rule-form-drawer"
			append-to-body
			destroy-on-close
		>
			<ConsoleDrawerWorkspace
				:eyebrow="isEditMode ? 'Rule Edit' : 'Rule Setup'"
				:title="state.dialogtitle"
				:description="drawerDescription"
				:badges="badges"
				:metrics="metrics"
			>
				<template #actions>
					<el-button @click="closeDialog">取消</el-button>
					<el-button type="primary" :loading="submitting" @click="onSubmit">保存规则</el-button>
				</template>

				<section class="rule-form-layout">
					<article class="form-card form-card--main">
						<div class="form-card__header">
							<div>
								<h3>规则基础信息</h3>
								<p>先确定规则名称和触发类型，后续再进入设计器补充节点流转、脚本和执行器逻辑。</p>
							</div>
						</div>

						<el-form ref="dataFormRef" :model="state.dataForm" :rules="rules" label-position="top">
							<el-form-item label="规则名称" prop="name">
								<el-input v-model="state.dataForm.name" placeholder="请输入规则名称" clearable />
							</el-form-item>

							<el-form-item label="事件类型" prop="mountType">
								<el-select v-model="state.dataForm.mountType" placeholder="请选择事件类型">
									<el-option
										v-for="item in mountTypeOptions"
										:key="item.value"
										:label="item.label"
										:value="item.value"
									/>
								</el-select>
							</el-form-item>

							<el-form-item label="规则说明" prop="ruleDesc">
								<el-input
									v-model="state.dataForm.ruleDesc"
									type="textarea"
									:rows="5"
									placeholder="补充规则作用、触发背景或主要处理目标。"
									clearable
								/>
							</el-form-item>
						</el-form>
					</article>

					<aside class="form-card">
						<div class="form-card__header">
							<div>
								<h3>配置建议</h3>
								<p>触发类型决定规则入口，建议先从最核心的事件开始，再在设计器里补充分支和执行动作。</p>
							</div>
						</div>
						<ul class="rule-tips">
							<li><strong>Telemetry</strong> 适合设备实时上报后的规则处理。</li>
							<li><strong>Attribute</strong> 适合配置、状态更新和静态信息同步。</li>
							<li><strong>RPC</strong> 适合远程控制和下行命令触发。</li>
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
import { NIL as NIL_UUID } from 'uuid';
import type { appmessage } from '/@/api/iapiresult';
import type { ruleaddoreditdto } from '/@/dtos/ruleaddoreditdtodto';
import { ruleApi } from '/@/api/flows';
import ConsoleDrawerWorkspace from '/@/components/console/ConsoleDrawerWorkspace.vue';

interface RuleFormState {
	drawer: boolean;
	dialogtitle: string;
	dataForm: ruleaddoreditdto;
}

const mountTypeOptions = [
	{ value: 'None', label: '无（None）' },
	{ value: 'RAW', label: '原始值（RAW）' },
	{ value: 'Telemetry', label: '遥测' },
	{ value: 'Attribute', label: '属性' },
	{ value: 'RPC', label: '远程控制（RPC）' },
	{ value: 'Connected', label: '在线' },
	{ value: 'Disconnected', label: '离线' },
	{ value: 'TelemetryArray', label: '遥测数组' },
	{ value: 'Alarm', label: '告警' },
	{ value: 'DeleteDevice', label: '删除设备' },
	{ value: 'CreateDevice', label: '创建设备' },
	{ value: 'Activity', label: '活动事件' },
	{ value: 'Inactivity', label: '非活动状态' },
];

const emit = defineEmits(['close', 'submit']);
const dataFormRef = ref<FormInstance>();
const submitting = ref(false);

const createDefaultRule = (): ruleaddoreditdto => ({
	ruleId: NIL_UUID,
	name: '',
	ruleDesc: '',
	mountType: '',
});

const rules = reactive<FormRules>({
	name: [
		{ required: true, message: '请输入规则名称', trigger: 'blur' },
		{ min: 2, message: '规则名称至少 2 个字符', trigger: 'blur' },
	],
	mountType: [{ required: true, message: '请选择事件类型', trigger: 'change' }],
});

const state = reactive<RuleFormState>({
	drawer: false,
	dialogtitle: '',
	dataForm: createDefaultRule(),
});

const isEditMode = computed(() => state.dataForm.ruleId !== NIL_UUID);
const mountTypeLabel = computed(() => {
	return mountTypeOptions.find((item) => item.value === state.dataForm.mountType)?.label || '未设置';
});
const drawerDescription = computed(() => {
	return isEditMode.value
		? '继续完善规则基础信息，保存后可以回到设计器继续编排节点。'
		: '先创建规则入口，再进入设计器补充具体的节点、脚本和执行器。';
});

const badges = computed(() => [
	isEditMode.value ? '编辑模式' : '新建模式',
	mountTypeLabel.value,
	state.dataForm.name || '未命名规则',
]);

const metrics = computed(() => [
	{
		label: '触发类型',
		value: mountTypeLabel.value,
		hint: '决定规则的入口事件和触发场景。',
		tone: 'primary' as const,
	},
	{
		label: '命名状态',
		value: state.dataForm.name ? '已填写' : '待填写',
		hint: '建议用业务可识别的名称标记规则目的。',
		tone: 'accent' as const,
	},
	{
		label: '设计阶段',
		value: isEditMode.value ? '继续完善' : '准备创建',
		hint: '基础信息保存后即可进入规则设计器。',
		tone: 'success' as const,
	},
	{
		label: '说明状态',
		value: state.dataForm.ruleDesc ? '已补充' : '可选补充',
		hint: '描述规则背景有助于后续维护和排查。',
		tone: 'warning' as const,
	},
]);

const openDialog = async (ruleid: string) => {
	state.drawer = true;

	if (ruleid === NIL_UUID) {
		state.dataForm = createDefaultRule();
		state.dialogtitle = '新建规则';
		await nextTick();
		dataFormRef.value?.clearValidate();
		return;
	}

	state.dialogtitle = '编辑规则';
	const response = await ruleApi().getrule(ruleid);
	state.dataForm = response.data;
	await nextTick();
	dataFormRef.value?.clearValidate();
};

const closeDialog = () => {
	state.drawer = false;
	emit('close', state.dataForm);
};

const onSubmit = async () => {
	if (submitting.value) return;

	const form = dataFormRef.value;
	if (!form) return;

	await form.validate(async (valid) => {
		if (!valid) return;

		submitting.value = true;
		try {
			if (state.dataForm.ruleId === NIL_UUID) {
				const response: appmessage<boolean> = await ruleApi().postrule(state.dataForm);
				if (response.code === 10000 && response.data) {
					ElMessage.success('规则创建成功');
					emit('submit', state.dataForm);
					emit('close', state.dataForm);
					state.drawer = false;
				}
				else {
					ElMessage.warning(`规则创建失败: ${response.msg}`);
				}
			}
			else {
				const response: appmessage<boolean> = await ruleApi().putrule(state.dataForm);
				if (response.code === 10000 && response.data) {
					ElMessage.success('规则更新成功');
					emit('close', state.dataForm);
					state.drawer = false;
				}
				else {
					ElMessage.warning(`规则更新失败: ${response.msg}`);
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
:deep(.rule-form-drawer .el-drawer__header) {
	margin-bottom: 0;
	padding-bottom: 0;
}

:deep(.rule-form-drawer .el-drawer__body) {
	padding: 18px;
	background: linear-gradient(180deg, #f8fbff 0%, #f3f7fc 100%);
}

.rule-form-layout {
	display: grid;
	grid-template-columns: minmax(0, 1.5fr) 320px;
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

.form-card :deep(.el-input__wrapper),
.form-card :deep(.el-select__wrapper),
.form-card :deep(.el-textarea__inner) {
	border-radius: 16px;
}

.rule-tips {
	margin: 0;
	padding-left: 18px;
	color: #475569;
	font-size: 13px;
	line-height: 1.8;
}

@media (max-width: 1080px) {
	.rule-form-layout {
		grid-template-columns: 1fr;
	}
}

@media (max-width: 767px) {
	:deep(.rule-form-drawer) {
		width: 100% !important;
	}

	:deep(.rule-form-drawer .el-drawer__body) {
		padding: 12px;
	}

	.form-card {
		padding: 16px;
		border-radius: 20px;
	}
}
</style>
