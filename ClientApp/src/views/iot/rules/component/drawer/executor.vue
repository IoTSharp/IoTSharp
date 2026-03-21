<template>
	<div class="drawer-panel">
		<section class="drawer-panel__grid">
			<article class="drawer-card drawer-card--editor">
				<div class="drawer-card__header">
					<div>
						<h3>执行器配置</h3>
						<p>在这里编辑执行器的 JSON 配置，并结合右侧说明确认当前处理器的输入输出约束。</p>
					</div>
					<el-button plain @click="applySample">填入示例</el-button>
				</div>

				<div class="drawer-editor">
					<monaco
						height="460px"
						width="100%"
						theme="vs-dark"
						v-model="state.node.content"
						language="json"
						selectOnLineNumbers="true"
					/>
				</div>
			</article>

			<aside class="drawer-side">
				<article class="drawer-card">
					<div class="drawer-card__header">
						<div>
							<h3>节点信息</h3>
							<p>先确定节点名称，再确认当前绑定的是哪个处理器实现，避免后续配置错位。</p>
						</div>
					</div>

					<el-form ref="formRef" :model="state.node" :rules="rules" label-position="top">
						<el-form-item label="节点名称" prop="name">
							<el-input v-model="state.node.name" placeholder="请输入节点名称" clearable />
						</el-form-item>
						<el-form-item label="处理器实现">
							<el-input :model-value="state.node.mata" disabled />
						</el-form-item>
						<el-form-item label="节点 ID">
							<el-input :model-value="state.node.nodeId" disabled />
						</el-form-item>
					</el-form>
				</article>

				<article class="drawer-card">
					<div class="drawer-card__header">
						<div>
							<h3>{{ currentDoc.title }}</h3>
							<p>{{ currentDoc.description }}</p>
						</div>
					</div>

					<ul class="drawer-tips">
						<li v-for="item in currentDoc.highlights" :key="item">{{ item }}</li>
					</ul>

					<div class="drawer-sample">
						<div class="drawer-sample__label">配置示例</div>
						<pre>{{ currentDoc.sample }}</pre>
					</div>
				</article>
			</aside>
		</section>

		<div class="drawer-actions">
			<el-button @click="$emit('close')">取消</el-button>
			<el-button type="primary" @click="submitPanel">保存配置</el-button>
		</div>
	</div>
</template>

<script lang="ts" setup>
import { computed, reactive, ref, watch } from 'vue';
import { ElMessage, type FormInstance, type FormRules } from 'element-plus';
import monaco from '/@/components/monaco/monaco.vue';
import { defaultExecutorDoc, executorDocs } from './executorDocs';

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

const currentDoc = computed(() => executorDocs[state.node.mata] ?? defaultExecutorDoc);

watch(
	() => props.modelValue,
	(value) => {
		state.node = { ...(value ?? {}) };
	},
	{ deep: true }
);

const applySample = () => {
	if (!state.node.content?.trim()) {
		state.node.content = currentDoc.value.sample;
		return;
	}

	state.node.content = currentDoc.value.sample;
};

const submitPanel = async () => {
	if (!formRef.value) return;

	await formRef.value.validate(async (valid) => {
		if (!valid) return;

		if (state.node.content?.trim()) {
			try {
				JSON.parse(state.node.content);
			}
			catch {
				ElMessage.warning('执行器配置需要是合法的 JSON');
				return;
			}
		}

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

.drawer-sample {
	margin-top: 16px;
	padding: 14px 16px;
	border-radius: 18px;
	background: rgba(248, 250, 252, 0.9);
	border: 1px solid rgba(226, 232, 240, 0.92);
}

.drawer-sample__label {
	margin-bottom: 8px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.08em;
	text-transform: uppercase;
}

.drawer-sample pre {
	margin: 0;
	padding: 12px;
	border-radius: 14px;
	background: #0f172a;
	color: #cbd5e1;
	font-size: 12px;
	line-height: 1.7;
	white-space: pre-wrap;
	word-break: break-word;
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
