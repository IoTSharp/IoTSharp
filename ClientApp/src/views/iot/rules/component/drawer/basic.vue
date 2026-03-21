<template>
	<div class="drawer-panel">
		<section class="drawer-panel__grid">
			<article class="drawer-card drawer-card--main">
				<div class="drawer-card__header">
					<div>
						<h3>基础节点信息</h3>
						<p>开始和结束节点承担规则流程边界的语义，建议保持名称直接、稳定，方便团队快速理解流程入口与出口。</p>
					</div>
				</div>

				<el-form ref="formRef" :model="state.node" :rules="rules" label-position="top">
					<el-form-item label="节点名称" prop="name">
						<el-input v-model="state.node.name" placeholder="请输入节点名称" clearable />
					</el-form-item>

					<el-row :gutter="16">
						<el-col :xs="24" :md="12">
							<el-form-item label="节点类型">
								<el-input :model-value="nodeTypeLabel" disabled />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :md="12">
							<el-form-item label="BPMN 命名空间">
								<el-input :model-value="state.node.nodenamespace" disabled />
							</el-form-item>
						</el-col>
					</el-row>

					<el-row :gutter="16">
						<el-col :xs="24" :md="12">
							<el-form-item label="节点 ID">
								<el-input :model-value="state.node.nodeId" disabled />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :md="12">
							<el-form-item label="内部标识">
								<el-input :model-value="state.node.id" disabled />
							</el-form-item>
						</el-col>
					</el-row>
				</el-form>
			</article>

			<aside class="drawer-card">
				<div class="drawer-card__header">
					<div>
						<h3>节点建议</h3>
						<p>基础节点没有复杂配置，重点是保持流程语义清楚，避免名称泛化。</p>
					</div>
				</div>
				<ul class="drawer-tips">
					<li>开始节点建议直接描述入口事件，例如“遥测入口”或“属性同步开始”。</li>
					<li>结束节点建议描述最终结果，例如“发布成功”或“流程终止”。</li>
					<li>如果后续会增加分支，优先在连线条件里表达逻辑，不要把节点名写得太长。</li>
				</ul>
			</aside>
		</section>

		<div class="drawer-actions">
			<el-button @click="$emit('close')">取消</el-button>
			<el-button type="primary" @click="submitPanel">保存节点</el-button>
		</div>
	</div>
</template>

<script lang="ts" setup>
import { computed, reactive, ref, watch } from 'vue';
import type { FormInstance, FormRules } from 'element-plus';

const props = defineProps({
	modelValue: {
		type: Object,
		default: () => ({}),
	},
});

const emit = defineEmits(['close', 'submit']);
const formRef = ref<FormInstance>();

const state = reactive({
	node: { ...(props.modelValue ?? {}) } as Record<string, any>,
});

const rules = reactive<FormRules>({
	name: [
		{ required: true, message: '请输入节点名称', trigger: 'blur' },
		{ min: 2, message: '节点名称至少 2 个字符', trigger: 'blur' },
	],
});

const nodeTypeLabel = computed(() => {
	if (state.node.mata === 'begin' || state.node.nodenamespace === 'bpmn:StartEvent') return '开始节点';
	if (state.node.mata === 'end' || state.node.nodenamespace === 'bpmn:EndEvent') return '结束节点';
	return '基础节点';
});

watch(
	() => props.modelValue,
	(value) => {
		state.node = { ...(value ?? {}) };
	},
	{ deep: true }
);

const submitPanel = async () => {
	if (!formRef.value) return;

	await formRef.value.validate((valid) => {
		if (!valid) return;
		emit('submit', state.node);
	});
};
</script>

<style scoped lang="scss">
.drawer-panel {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.drawer-panel__grid {
	display: grid;
	grid-template-columns: minmax(0, 1.45fr) 280px;
	gap: 18px;
}

.drawer-card {
	padding: 20px;
	border-radius: 22px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background:
		radial-gradient(circle at top right, rgba(59, 130, 246, 0.08), transparent 30%),
		linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.96));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.drawer-card__header {
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

.drawer-card :deep(.el-input__wrapper) {
	border-radius: 16px;
}

.drawer-tips {
	margin: 0;
	padding-left: 18px;
	color: #475569;
	font-size: 13px;
	line-height: 1.8;
}

.drawer-actions {
	display: flex;
	justify-content: flex-end;
	gap: 10px;
}

.drawer-actions :deep(.el-button) {
	min-width: 108px;
	height: 42px;
	border-radius: 14px;
}

@media (max-width: 1080px) {
	.drawer-panel__grid {
		grid-template-columns: 1fr;
	}
}

@media (max-width: 767px) {
	.drawer-card {
		padding: 16px;
		border-radius: 20px;
	}

	.drawer-actions {
		flex-direction: column-reverse;
	}

	.drawer-actions :deep(.el-button) {
		width: 100%;
	}
}
</style>
