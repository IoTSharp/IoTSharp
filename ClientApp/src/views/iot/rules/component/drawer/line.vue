<template>
	<div class="drawer-panel">
		<section class="drawer-panel__grid">
			<article class="drawer-card drawer-card--main">
				<div class="drawer-card__header">
					<div>
						<h3>连线条件</h3>
						<p>用显示名称提升画布可读性，用条件表达式约束流转路径，帮助分支逻辑更直观。</p>
					</div>
				</div>

				<el-form :model="state.line" label-position="top">
					<el-form-item label="流转路径">
						<el-input v-model="state.line.contact" disabled />
					</el-form-item>
					<el-row :gutter="16">
						<el-col :xs="24" :md="12">
							<el-form-item label="显示名称">
								<el-input v-model="state.line.linename" placeholder="例如：温度过高" clearable />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :md="12">
							<el-form-item label="连线类型">
								<el-input v-model="state.line.type" disabled />
							</el-form-item>
						</el-col>
					</el-row>
					<el-form-item label="条件表达式">
						<el-input
							v-model="state.line.condition"
							type="textarea"
							:rows="6"
							placeholder="例如：Age > 18"
							clearable
						/>
					</el-form-item>
				</el-form>
			</article>

			<aside class="drawer-card">
				<div class="drawer-card__header">
					<div>
						<h3>表达式提示</h3>
						<p>条件表达式直接使用字段名，不需要再拼接 `Input.` 前缀。</p>
					</div>
				</div>

				<ul class="drawer-tips">
					<li>如果上游节点输出 `{ "Name": "张三", "Age": 18 }`，条件可以直接写成 `Age > 18`。</li>
					<li>显示名称会直接展示在连线上，适合写成“通过”“失败”“命中阈值”这类短标签。</li>
					<li>条件尽量保持单一判断，复杂逻辑建议前置到脚本节点中处理。</li>
				</ul>

				<div class="drawer-sample">
					<div class="drawer-sample__label">输入示例</div>
					<pre>{
  "Name": "张三",
  "Age": 18
}</pre>
				</div>
			</aside>
		</section>

		<div class="drawer-actions">
			<el-button @click="$emit('close')">取消</el-button>
			<el-button type="primary" @click="submitPanel">保存连线</el-button>
		</div>
	</div>
</template>

<script lang="ts" setup>
import { reactive, watch } from 'vue';

const props = defineProps({
	modelValue: {
		type: Object,
		default: () => ({}),
	},
});

const emit = defineEmits(['submit', 'close']);

const state = reactive({
	line: { ...(props.modelValue ?? {}) } as Record<string, any>,
});

watch(
	() => props.modelValue,
	(value) => {
		state.line = { ...(value ?? {}) };
	},
	{ deep: true }
);

const submitPanel = () => {
	emit('submit', state.line);
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

.drawer-card :deep(.el-input__wrapper),
.drawer-card :deep(.el-textarea__inner) {
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
