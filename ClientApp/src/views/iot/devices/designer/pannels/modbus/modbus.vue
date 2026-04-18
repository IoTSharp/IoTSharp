<template>
	<div class="protocol-workspace">
		<div class="protocol-workspace__hero">
			<div>
				<h3>Modbus 采集设计</h3>
				<p>按连接、设备、点位、转换、映射、预览六段式组织配置，替代旧的单表格编辑。</p>
			</div>
			<el-button type="primary" @click="save">保存草案</el-button>
		</div>

		<el-collapse v-model="activeSections">
			<el-collapse-item title="1. 连接配置" name="connection">
				<el-form label-width="140px">
					<el-form-item label="连接名称">
						<el-input v-model="connection.connectionName" />
					</el-form-item>
					<el-form-item label="传输方式">
						<el-select v-model="connection.transport">
							<el-option label="Modbus TCP" value="Tcp" />
							<el-option label="RTU over TCP" value="RtuOverTcp" />
							<el-option label="串口 RTU" value="SerialRtu" />
						</el-select>
					</el-form-item>
					<el-form-item label="Host">
						<el-input v-model="connection.host" />
					</el-form-item>
					<el-form-item label="Port">
						<el-input-number v-model="connection.port" :min="1" :max="65535" />
					</el-form-item>
				</el-form>
			</el-collapse-item>

			<el-collapse-item title="2. 设备分组" name="devices">
				<el-alert title="当前先保留单设备骨架，后续扩展多从站批量编辑。" type="info" :closable="false" />
				<el-form label-width="140px" class="protocol-workspace__form">
					<el-form-item label="设备名称">
						<el-input v-model="device.deviceName" />
					</el-form-item>
					<el-form-item label="从站编号">
						<el-input-number v-model="device.protocolOptions.slaveId" :min="1" :max="247" />
					</el-form-item>
				</el-form>
			</el-collapse-item>

			<el-collapse-item title="3. 点位清单" name="points">
				<modbuspointlist v-model="props.modelValue" @submit="forwardLegacySubmit" />
			</el-collapse-item>

			<el-collapse-item title="4. 转换规则" name="transforms">
				<el-form label-width="140px">
					<el-form-item label="缩放系数">
						<el-input-number v-model="pointDraft.scale" :step="0.1" />
					</el-form-item>
					<el-form-item label="偏移量">
						<el-input-number v-model="pointDraft.offset" :step="0.1" />
					</el-form-item>
					<el-form-item label="表达式">
						<el-input v-model="pointDraft.expression" placeholder="例如 (x * 0.1) + 5" />
					</el-form-item>
				</el-form>
			</el-collapse-item>

			<el-collapse-item title="5. 平台映射" name="mapping">
				<el-form label-width="140px">
					<el-form-item label="目标类型">
						<el-select v-model="pointDraft.mapping.targetType">
							<el-option label="遥测" value="Telemetry" />
							<el-option label="属性" value="Attribute" />
							<el-option label="告警输入" value="AlarmInput" />
						</el-select>
					</el-form-item>
					<el-form-item label="平台属性名">
						<el-input v-model="pointDraft.mapping.targetName" />
					</el-form-item>
					<el-form-item label="值类型">
						<el-select v-model="pointDraft.mapping.valueType">
							<el-option label="Boolean" value="Boolean" />
							<el-option label="Int32" value="Int32" />
							<el-option label="Double" value="Double" />
							<el-option label="String" value="String" />
						</el-select>
					</el-form-item>
					<el-form-item label="单位">
						<el-input v-model="pointDraft.mapping.unit" />
					</el-form-item>
				</el-form>
			</el-collapse-item>

			<el-collapse-item title="6. 试读预览" name="preview">
				<el-descriptions :column="2" border>
					<el-descriptions-item label="原始值">{{ preview.rawValue }}</el-descriptions-item>
					<el-descriptions-item label="转换后值">{{ preview.transformedValue }}</el-descriptions-item>
					<el-descriptions-item label="质量">{{ preview.qualityStatus }}</el-descriptions-item>
					<el-descriptions-item label="值类型">{{ preview.valueType }}</el-descriptions-item>
				</el-descriptions>
				<div class="protocol-workspace__actions">
					<el-button @click="simulatePreview">试读预览</el-button>
				</div>
			</el-collapse-item>
		</el-collapse>
	</div>
</template>

<script lang="ts" setup>
import { reactive, ref } from 'vue';
import modbuspointlist from './modbuspointlist.vue';
import type { ModbusConnectionModel, ModbusDeviceModel, ModbusPointModel } from '../../models/modbus-task';

const props = defineProps({
	modelValue: {
		type: Object,
		default: () => ({}),
	},
});

const emit = defineEmits(['submit']);

const activeSections = ref(['connection', 'devices', 'points', 'mapping']);

const connection = reactive<ModbusConnectionModel>({
	connectionKey: 'default-connection',
	connectionName: '默认连接',
	protocol: 'Modbus',
	transport: 'Tcp',
	host: '127.0.0.1',
	port: 502,
	timeoutMs: 3000,
	retryCount: 3,
	protocolOptions: {
		maxBatchRegisters: 64,
		maxConcurrentRequests: 1,
	},
});

const device = reactive<ModbusDeviceModel>({
	deviceKey: 'device-1',
	deviceName: '示例 Modbus 设备',
	enabled: true,
	points: [],
	protocolOptions: {
		slaveId: 1,
	},
});

const pointDraft = reactive<ModbusPointModel>({
	pointKey: 'point-1',
	pointName: '示例点位',
	sourceType: 'HoldingRegister',
	address: '40001',
	rawValueType: 'UInt16',
	length: 1,
	polling: {
		readPeriodMs: 5000,
		group: 'default',
	},
	transforms: [],
	mapping: {
		targetType: 'Telemetry',
		targetName: 'demoValue',
		valueType: 'Double',
		displayName: '示例值',
		unit: '°C',
		group: 'default',
	},
	protocolOptions: {
		slaveId: 1,
		functionCode: 3,
		area: 'HoldingRegister',
		registerCount: 1,
		byteOrder: 'AB',
		wordOrder: 'AB',
	},
	scale: 0.1,
	offset: 0,
	expression: '',
});

const preview = reactive({
	rawValue: '--',
	transformedValue: '--',
	qualityStatus: 'Good',
	valueType: 'Double',
});

const simulatePreview = () => {
	preview.rawValue = '1234';
	preview.transformedValue = pointDraft.scale ? String(1234 * pointDraft.scale) : '1234';
	preview.qualityStatus = 'Good';
	preview.valueType = pointDraft.mapping.valueType;
};

const forwardLegacySubmit = (payload: any) => {
	emit('submit', payload);
};

const save = () => {
	emit('submit', {
		namespace: 'modbusworkspacechanged',
		data: {
			sender: props.modelValue,
			connection: { ...connection },
			device: { ...device },
			pointDraft: { ...pointDraft },
		},
	});
};
</script>

<style scoped>
.protocol-workspace {
	padding: 16px;
}

.protocol-workspace__hero {
	display: flex;
	justify-content: space-between;
	align-items: flex-start;
	gap: 16px;
	margin-bottom: 20px;
}

.protocol-workspace__hero h3 {
	margin: 0 0 8px;
}

.protocol-workspace__hero p {
	margin: 0;
	color: #64748b;
}

.protocol-workspace__form,
.protocol-workspace__actions {
	margin-top: 16px;
}
</style>
