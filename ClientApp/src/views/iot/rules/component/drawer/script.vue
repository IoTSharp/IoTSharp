<template>
	<div class="drawer-panel">
		<section class="drawer-panel__grid">
			<article class="drawer-card drawer-card--editor">
				<div class="drawer-card__header">
					<div>
						<h3>脚本编辑区</h3>
						<p>在这里直接编写脚本逻辑，适合做数据清洗、字段转换、条件判断和自定义输出拼装。</p>
					</div>
					<el-tag effect="plain" type="primary">{{ languageLabel }}</el-tag>
				</div>

				<div class="drawer-editor">
					<monaco
						height="460px"
						width="100%"
						theme="vs-dark"
						v-model="state.node.content"
						:language="editorLanguage"
						selectOnLineNumbers="true"
					/>
				</div>
			</article>

			<aside class="drawer-side">
				<article class="drawer-card">
					<div class="drawer-card__header">
						<div>
							<h3>节点信息</h3>
							<p>维护脚本节点名称，并确认运行时、节点 ID 等基础信息。</p>
						</div>
					</div>

					<el-form ref="formRef" :model="state.node" :rules="rules" label-position="top">
						<el-form-item label="节点名称" prop="name">
							<el-input v-model="state.node.name" placeholder="请输入节点名称" clearable />
						</el-form-item>
						<el-form-item label="脚本运行时">
							<el-input :model-value="languageLabel" disabled />
						</el-form-item>
						<el-form-item label="节点 ID">
							<el-input :model-value="state.node.nodeId" disabled />
						</el-form-item>
						<el-form-item label="BPMN 命名空间">
							<el-input :model-value="state.node.nodenamespace" disabled />
						</el-form-item>
					</el-form>
				</article>

				<article class="drawer-card">
					<div class="drawer-card__header">
						<div>
							<h3>编写提示</h3>
							<p>保持脚本节点职责单一，会让整条规则链路更容易回看和排障。</p>
						</div>
					</div>
					<ul class="drawer-tips">
						<li>推荐让一个脚本节点只负责一种转换或判断，避免逻辑过度堆积。</li>
						<li>如果脚本会输出给下游执行器，尽量保持输出结构稳定。</li>
						<li>复杂逻辑优先拆成多个脚本节点，再通过连线条件组织流程。</li>
					</ul>
				</article>
			</aside>
		</section>

		<div class="drawer-actions">
			<el-button @click="$emit('close')">取消</el-button>
			<el-button type="primary" @click="submitPanel">保存脚本</el-button>
		</div>
	</div>
</template>

<script lang="ts" setup>
import { computed, reactive, ref, watch } from 'vue';
import { ElMessage, type FormInstance, type FormRules } from 'element-plus';
import monaco from '/@/components/monaco/monaco.vue';

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

const languageMap: Record<string, { editor: string; label: string }> = {
	javascript: { editor: 'javascript', label: 'JavaScript' },
	python: { editor: 'python', label: 'Python' },
	sql: { editor: 'sql', label: 'SQL' },
	lua: { editor: 'lua', label: 'Lua' },
	csharp: { editor: 'csharp', label: 'C#' },
};

const rules = reactive<FormRules>({
	name: [
		{ required: true, message: '请输入节点名称', trigger: 'blur' },
		{ min: 2, message: '节点名称至少 2 个字符', trigger: 'blur' },
	],
	content: [{ required: true, message: '请编写脚本内容', trigger: 'blur' }],
});

const editorLanguage = computed(() => {
	return languageMap[state.node.mata]?.editor || 'javascript';
});

const languageLabel = computed(() => {
	return languageMap[state.node.mata]?.label || state.node.mata || 'Script';
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
	if (!state.node.content?.trim()) {
		ElMessage.warning('请先填写脚本内容');
		return;
	}

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
	grid-template-columns: minmax(0, 1.6fr) 320px;
	gap: 18px;
}

.drawer-side {
	display: flex;
	flex-direction: column;
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
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 12px;
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

.drawer-editor {
	border-radius: 18px;
	overflow: hidden;
	border: 1px solid rgba(226, 232, 240, 0.92);
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
