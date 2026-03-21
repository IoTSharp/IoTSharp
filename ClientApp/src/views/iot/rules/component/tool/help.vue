<template>
	<div class="workflow-tool-help">
		<el-dialog v-model="isShow" width="920px" class="workflow-tool-help__dialog" destroy-on-close>
			<template #header>
				<div v-drag="['.workflow-tool-help .el-dialog', '.workflow-tool-help__header']" class="workflow-tool-help__header">
					<div class="workflow-tool-help__header-copy">
						<span class="workflow-tool-help__eyebrow">Rule Designer Guide</span>
						<strong>使用帮助</strong>
						<small>把拖拽建模、条件配置和保存导出整合成一套更顺手的工作流。</small>
					</div>
					<span class="workflow-tool-help__badge">5 个关键步骤</span>
				</div>
			</template>

			<div class="workflow-tool-help__body">
				<section class="workflow-tool-help__hero">
					<article v-for="card in heroCards" :key="card.title" class="workflow-tool-help__hero-card">
						<span>{{ card.title }}</span>
						<strong>{{ card.highlight }}</strong>
						<p>{{ card.description }}</p>
					</article>
				</section>

				<section class="workflow-tool-help__section">
					<div class="workflow-tool-help__section-title">
						<span>开始使用</span>
						<small>第一次进入规则设计器时，可以直接按下面的顺序完成建模。</small>
					</div>

					<div class="workflow-tool-help__steps">
						<article v-for="step in helpSteps" :key="step.title" class="workflow-tool-help__step">
							<div class="workflow-tool-help__step-index">{{ step.index }}</div>
							<div class="workflow-tool-help__step-copy">
								<strong>{{ step.title }}</strong>
								<p>{{ step.description }}</p>
							</div>
						</article>
					</div>
				</section>

				<section class="workflow-tool-help__section workflow-tool-help__section--aside">
					<article class="workflow-tool-help__panel">
						<div class="workflow-tool-help__section-title">
							<span>工具栏说明</span>
							<small>顶部工具栏负责当前规则的常用操作。</small>
						</div>

						<div class="workflow-tool-help__chips">
							<span v-for="action in toolHighlights" :key="action">{{ action }}</span>
						</div>
					</article>

					<article class="workflow-tool-help__panel">
						<div class="workflow-tool-help__section-title">
							<span>建模建议</span>
							<small>这几条能帮我们减少误操作和重复排查。</small>
						</div>

						<ul class="workflow-tool-help__tips">
							<li v-for="tip in helpTips" :key="tip">{{ tip }}</li>
						</ul>
					</article>
				</section>
			</div>

			<template #footer>
				<div class="workflow-tool-help__footer">
					<el-button @click="close">稍后再看</el-button>
					<el-button type="primary" @click="close">我知道了</el-button>
				</div>
			</template>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup>
import { ref } from 'vue';

const isShow = ref(false);

const heroCards = [
	{
		title: '拖拽建模',
		highlight: '从左侧节点库直接拖到画布',
		description: '按分类展开节点后，拖入右侧画布即可开始编排，节点位置支持继续微调。',
	},
	{
		title: '右键配置',
		highlight: '节点和连线都支持独立配置',
		description: '右键节点可编辑参数，右键连线可配置条件，让结构和逻辑都保持清晰。',
	},
	{
		title: '安全保存',
		highlight: '先预览再保存',
		description: '保存前建议先检查连线路径和节点参数，必要时先导出 JSON 备份当前编排。',
	},
];

const helpSteps = [
	{
		index: '01',
		title: '展开节点库',
		description: '先从左侧分类中找到需要的基础节点、执行器节点或脚本节点，再开始拖拽。',
	},
	{
		index: '02',
		title: '拖入画布排布结构',
		description: '把节点拖入右侧网格画布，按照业务执行顺序摆放，后续阅读和维护会更轻松。',
	},
	{
		index: '03',
		title: '连接节点关系',
		description: '从节点图标位置拉出连线，连接到目标节点，重复连接会被自动拦截。',
	},
	{
		index: '04',
		title: '补齐配置与条件',
		description: '右键节点或连线进入配置面板，填写脚本、执行器参数、条件表达式等信息。',
	},
	{
		index: '05',
		title: '保存并回到列表',
		description: '确认节点、连线和条件无误后保存规则，必要时先复制或下载当前 JSON 方案。',
	},
];

const toolHighlights = ['帮助', '下载', '保存', '复制 JSON', '清空画布', '全屏', '返回列表'];

const helpTips = [
	'优先先摆结构，再补细节配置，这样节点关系不容易改乱。',
	'连线没有名称时会隐藏标签，建议关键分支都补上可读名称。',
	'删除节点会一起移除关联连线，正式保存前最好导出一次备份。',
	'移动端当前仅适合查看，不适合拖拽建模，请尽量在桌面端完成编辑。',
];

const open = () => {
	isShow.value = true;
};

const close = () => {
	isShow.value = false;
};

defineExpose({
	open,
	close,
});
</script>

<style scoped lang="scss">
.workflow-tool-help__header {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 16px;
	padding-right: 18px;
	cursor: move;
}

.workflow-tool-help__header-copy {
	min-width: 0;

	strong,
	small {
		display: block;
	}

	strong {
		color: #123b6d;
		font-size: 24px;
		font-weight: 700;
		letter-spacing: -0.04em;
	}

	small {
		margin-top: 8px;
		color: #64748b;
		font-size: 13px;
		line-height: 1.7;
	}
}

.workflow-tool-help__eyebrow {
	display: inline-flex;
	margin-bottom: 10px;
	color: #2563eb;
	font-size: 11px;
	font-weight: 700;
	letter-spacing: 0.14em;
	text-transform: uppercase;
}

.workflow-tool-help__badge {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	min-width: 108px;
	height: 34px;
	padding: 0 14px;
	border-radius: 999px;
	border: 1px solid rgba(191, 219, 254, 0.92);
	background: rgba(239, 246, 255, 0.84);
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	white-space: nowrap;
}

.workflow-tool-help__body {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.workflow-tool-help__hero {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 12px;
}

.workflow-tool-help__hero-card,
.workflow-tool-help__panel,
.workflow-tool-help__step {
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: rgba(255, 255, 255, 0.9);
	box-shadow: 0 14px 30px rgba(15, 23, 42, 0.04);
}

.workflow-tool-help__hero-card {
	padding: 18px;
	border-radius: 22px;
	background:
		radial-gradient(circle at top right, rgba(59, 130, 246, 0.08), transparent 32%),
		linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.96));

	span,
	strong {
		display: block;
	}

	span {
		color: #64748b;
		font-size: 12px;
	}

	strong {
		margin-top: 8px;
		color: #123b6d;
		font-size: 16px;
		line-height: 1.5;
	}

	p {
		margin: 10px 0 0;
		color: #5f7289;
		font-size: 13px;
		line-height: 1.8;
	}
}

.workflow-tool-help__section {
	display: flex;
	flex-direction: column;
	gap: 14px;
}

.workflow-tool-help__section--aside {
	display: grid;
	grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
	gap: 14px;
}

.workflow-tool-help__section-title {
	span,
	small {
		display: block;
	}

	span {
		color: #123b6d;
		font-size: 18px;
		font-weight: 700;
		letter-spacing: -0.03em;
	}

	small {
		margin-top: 6px;
		color: #64748b;
		font-size: 12px;
		line-height: 1.6;
	}
}

.workflow-tool-help__steps {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 12px;
}

.workflow-tool-help__step {
	display: flex;
	align-items: flex-start;
	gap: 14px;
	padding: 18px;
	border-radius: 22px;
}

.workflow-tool-help__step-index {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 44px;
	height: 44px;
	border-radius: 16px;
	background: linear-gradient(135deg, rgba(37, 99, 235, 0.16), rgba(59, 130, 246, 0.08));
	color: #2563eb;
	font-size: 14px;
	font-weight: 700;
	flex-shrink: 0;
}

.workflow-tool-help__step-copy {
	strong {
		display: block;
		color: #123b6d;
		font-size: 15px;
		font-weight: 700;
	}

	p {
		margin: 8px 0 0;
		color: #5f7289;
		font-size: 13px;
		line-height: 1.8;
	}
}

.workflow-tool-help__panel {
	padding: 18px;
	border-radius: 22px;
}

.workflow-tool-help__chips {
	display: flex;
	flex-wrap: wrap;
	gap: 10px;
	margin-top: 14px;

	span {
		display: inline-flex;
		align-items: center;
		min-height: 36px;
		padding: 0 14px;
		border-radius: 999px;
		border: 1px solid rgba(191, 219, 254, 0.9);
		background: rgba(239, 246, 255, 0.82);
		color: #2563eb;
		font-size: 12px;
		font-weight: 700;
	}
}

.workflow-tool-help__tips {
	margin: 14px 0 0;
	padding-left: 18px;
	color: #5f7289;
	font-size: 13px;
	line-height: 1.9;
}

.workflow-tool-help__footer {
	display: flex;
	justify-content: flex-end;
	gap: 10px;
}

.workflow-tool-help__dialog :deep(.el-dialog) {
	border-radius: 30px;
	border: 1px solid rgba(191, 219, 254, 0.78);
	background:
		radial-gradient(circle at top right, rgba(96, 165, 250, 0.14), transparent 28%),
		linear-gradient(180deg, rgba(248, 251, 255, 0.98), rgba(255, 255, 255, 0.98));
	box-shadow: 0 26px 56px rgba(15, 23, 42, 0.14);
	overflow: hidden;
}

.workflow-tool-help__dialog :deep(.el-dialog__header) {
	padding: 28px 30px 0;
	margin-right: 0;
}

.workflow-tool-help__dialog :deep(.el-dialog__body) {
	padding: 22px 30px 12px;
}

.workflow-tool-help__dialog :deep(.el-dialog__footer) {
	padding: 0 30px 28px;
}

@media (max-width: 960px) {
	.workflow-tool-help__hero,
	.workflow-tool-help__steps,
	.workflow-tool-help__section--aside {
		grid-template-columns: 1fr;
	}
}

@media (max-width: 767px) {
	.workflow-tool-help__header {
		flex-direction: column;
		padding-right: 0;
	}

	.workflow-tool-help__dialog :deep(.el-dialog) {
		width: calc(100vw - 20px) !important;
		border-radius: 24px;
	}

	.workflow-tool-help__dialog :deep(.el-dialog__header) {
		padding: 24px 20px 0;
	}

	.workflow-tool-help__dialog :deep(.el-dialog__body) {
		padding: 18px 20px 12px;
	}

	.workflow-tool-help__dialog :deep(.el-dialog__footer) {
		padding: 0 20px 22px;
	}

	.workflow-tool-help__footer {
		width: 100%;
	}

	.workflow-tool-help__footer :deep(.el-button) {
		flex: 1;
	}
}
</style>
