<template>
	<transition name="el-zoom-in-center">
		<div
			v-show="state.isShow"
			class="custom-contextmenu"
			role="menu"
			:style="menuStyle"
			@click.stop
			@contextmenu.prevent.stop
		>
			<div class="custom-contextmenu__hero">
				<div class="custom-contextmenu__copy">
					<span class="custom-contextmenu__eyebrow">Line Actions</span>
					<strong>{{ lineTitle }}</strong>
					<small>{{ state.item.condition || '尚未配置条件表达式' }}</small>
				</div>
				<span class="custom-contextmenu__badge">连线</span>
			</div>

			<div class="custom-contextmenu__meta">
				<div class="custom-contextmenu__meta-item">
					<span>起点</span>
					<strong>{{ state.conn.sourceId || '未识别' }}</strong>
				</div>
				<div class="custom-contextmenu__meta-item">
					<span>终点</span>
					<strong>{{ state.conn.targetId || '未识别' }}</strong>
				</div>
			</div>

			<button
				v-for="action in state.dropdownList"
				:key="action.contextMenuClickId"
				type="button"
				class="custom-contextmenu__action"
				:class="{ 'is-danger': action.contextMenuClickId === 0 }"
				@click="onCurrentClick(action.contextMenuClickId)"
			>
				<span class="custom-contextmenu__action-icon">
					<SvgIcon :name="action.icon" />
				</span>
				<span class="custom-contextmenu__action-copy">
					<strong>{{ action.txt }}</strong>
					<small>{{ action.description }}</small>
				</span>
			</button>
		</div>
	</transition>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, reactive } from 'vue';

interface DropdownState {
	x?: string | number;
	y?: string | number;
}

interface MenuAction {
	contextMenuClickId: number;
	txt: string;
	description: string;
	icon: string;
}

const props = withDefaults(
	defineProps<{
		dropdown?: DropdownState;
	}>(),
	{
		dropdown: () => ({
			x: 0,
			y: 0,
		}),
	}
);

const emit = defineEmits(['click', 'contextmenu', 'currentline']);

const state = reactive<{
	isShow: boolean;
	dropdownList: MenuAction[];
	item: Record<string, any>;
	conn: Record<string, any>;
}>({
	isShow: false,
	dropdownList: [
		{
			contextMenuClickId: 0,
			txt: '删除连线',
			description: '移除当前流程条件与连线路径',
			icon: 'ele-Delete',
		},
		{
			contextMenuClickId: 1,
			txt: '编辑连线',
			description: '继续设置名称、条件与触发逻辑',
			icon: 'ele-Edit',
		},
	],
	item: {
		type: 'line',
		value: '',
		label: '',
		condition: '',
	},
	conn: {},
});

const normalizeCoordinate = (value?: string | number) => {
	const parsed = Number(value ?? 0);
	return Number.isFinite(parsed) ? parsed : 0;
};

const menuStyle = computed(() => {
	const rawX = normalizeCoordinate(props.dropdown?.x);
	const rawY = normalizeCoordinate(props.dropdown?.y);

	if (typeof window === 'undefined') {
		return {
			top: `${rawY}px`,
			left: `${rawX}px`,
		};
	}

	const menuWidth = 312;
	const menuHeight = 264;
	const gap = 16;
	const left = Math.min(rawX, Math.max(gap, window.innerWidth - menuWidth - gap));
	const top = Math.min(rawY + 8, Math.max(gap, window.innerHeight - menuHeight - gap));

	return {
		top: `${top}px`,
		left: `${left}px`,
	};
});

const lineTitle = computed(() => {
	return state.conn.linename || state.item.linename || state.item.label || '未命名条件连线';
});

const onCurrentClick = (contextMenuClickId: number) => {
	emit(
		'currentline',
		{ contextMenuClickId, ...state.item, result: state.item.value },
		state.conn
	);
};

const openContextmenu = (item: Record<string, any>, conn = {}) => {
	state.item = {
		type: 'line',
		value: '',
		label: '',
		condition: '',
		...item,
	};
	state.conn = conn;
	closeContextmenu();
	window.setTimeout(() => {
		state.isShow = true;
	}, 10);
};

const closeContextmenu = () => {
	state.isShow = false;
};

onMounted(() => {
	document.body.addEventListener('click', closeContextmenu);
	document.body.addEventListener('contextmenu', closeContextmenu);
});

onUnmounted(() => {
	document.body.removeEventListener('click', closeContextmenu);
	document.body.removeEventListener('contextmenu', closeContextmenu);
});

defineExpose({
	openContextmenu,
});
</script>

<style scoped lang="scss">
.custom-contextmenu {
	position: fixed;
	z-index: 2190;
	display: flex;
	flex-direction: column;
	gap: 12px;
	width: min(312px, calc(100vw - 24px));
	padding: 16px;
	border-radius: 24px;
	border: 1px solid rgba(191, 219, 254, 0.92);
	background:
		radial-gradient(circle at top right, rgba(14, 165, 233, 0.14), transparent 34%),
		linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.96));
	box-shadow: 0 24px 48px rgba(15, 23, 42, 0.18);
	transform-origin: center top;
	backdrop-filter: blur(10px);
}

.custom-contextmenu__hero {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 12px;
}

.custom-contextmenu__copy {
	min-width: 0;

	strong,
	small {
		display: block;
	}

	strong {
		color: #123b6d;
		font-size: 16px;
		font-weight: 700;
		line-height: 1.4;
		word-break: break-word;
	}

	small {
		margin-top: 6px;
		color: #64748b;
		font-size: 12px;
		line-height: 1.6;
		word-break: break-word;
	}
}

.custom-contextmenu__eyebrow {
	display: inline-flex;
	margin-bottom: 8px;
	color: #0891b2;
	font-size: 11px;
	font-weight: 700;
	letter-spacing: 0.14em;
	text-transform: uppercase;
}

.custom-contextmenu__badge {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	min-width: 52px;
	height: 30px;
	padding: 0 12px;
	border-radius: 999px;
	border: 1px solid rgba(165, 243, 252, 0.92);
	background: rgba(236, 254, 255, 0.86);
	color: #0891b2;
	font-size: 12px;
	font-weight: 700;
	white-space: nowrap;
}

.custom-contextmenu__meta {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 10px;
}

.custom-contextmenu__meta-item {
	padding: 12px 14px;
	border-radius: 18px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: rgba(255, 255, 255, 0.84);

	span,
	strong {
		display: block;
	}

	span {
		color: #64748b;
		font-size: 11px;
	}

	strong {
		margin-top: 6px;
		color: #0f172a;
		font-size: 12px;
		line-height: 1.6;
		word-break: break-all;
	}
}

.custom-contextmenu__action {
	display: flex;
	align-items: center;
	gap: 12px;
	width: 100%;
	padding: 14px;
	border: 1px solid rgba(191, 219, 254, 0.82);
	border-radius: 18px;
	background: rgba(255, 255, 255, 0.84);
	color: inherit;
	cursor: pointer;
	transition: transform 0.18s ease, box-shadow 0.18s ease, border-color 0.18s ease, background-color 0.18s ease;
}

.custom-contextmenu__action:hover {
	transform: translateY(-1px);
	border-color: rgba(34, 211, 238, 0.96);
	background: rgba(236, 254, 255, 0.94);
	box-shadow: 0 16px 28px rgba(14, 165, 233, 0.14);
}

.custom-contextmenu__action.is-danger:hover {
	border-color: rgba(248, 113, 113, 0.9);
	background: rgba(254, 242, 242, 0.94);
	box-shadow: 0 16px 28px rgba(248, 113, 113, 0.14);
}

.custom-contextmenu__action-icon {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 40px;
	height: 40px;
	border-radius: 14px;
	background: rgba(207, 250, 254, 0.92);
	color: #0891b2;
	flex-shrink: 0;
}

.custom-contextmenu__action.is-danger .custom-contextmenu__action-icon {
	background: rgba(254, 226, 226, 0.88);
	color: #dc2626;
}

.custom-contextmenu__action-copy {
	display: flex;
	flex-direction: column;
	align-items: flex-start;
	min-width: 0;
}

.custom-contextmenu__action-copy strong {
	color: #123b6d;
	font-size: 14px;
	font-weight: 700;
}

.custom-contextmenu__action-copy small {
	margin-top: 4px;
	color: #64748b;
	font-size: 12px;
	line-height: 1.5;
	text-align: left;
}

@media (max-width: 767px) {
	.custom-contextmenu {
		padding: 14px;
		border-radius: 20px;
	}

	.custom-contextmenu__meta {
		grid-template-columns: 1fr;
	}
}
</style>
