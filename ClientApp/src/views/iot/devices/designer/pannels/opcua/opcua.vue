<template>
	<div class="protocol-workspace">
		<div class="protocol-workspace__hero">
			<div>
				<h3>OPC UA 采集设计</h3>
				<p>复用通用设计骨架，并保留 NodeId、订阅与采样周期等 OPC UA 特性。</p>
			</div>
			<el-button type="primary" @click="save">保存草案</el-button>
		</div>

		<el-collapse v-model="activeSections">
			<el-collapse-item title="1. 连接配置" name="connection">
				<el-form label-width="140px">
					<el-form-item label="连接名称">
						<el-input v-model="connection.connectionName" />
					</el-form-item>
					<el-form-item label="Endpoint">
						<el-input v-model="connection.protocolOptions.endpointUrl" />
					</el-form-item>
					<el-form-item label="SecurityMode">
						<el-select v-model="connection.protocolOptions.securityMode">
							<el-option label="None" value="None" />
							<el-option label="Sign" value="Sign" />
							<el-option label="SignAndEncrypt" value="SignAndEncrypt" />
						</el-select>
					</el-form-item>
				</el-form>
			</el-collapse-item>

			<el-collapse-item title="2. 设备分组" name="devices">
				<el-form label-width="140px">
					<el-form-item label="设备名称">
						<el-input v-model="device.deviceName" />
					</el-form-item>
					<el-form-item label="NamespaceIndex">
						<el-input-number v-model="device.protocolOptions.namespaceIndex" :min="0" />
					</el-form-item>
				</el-form>
			</el-collapse-item>

			<el-collapse-item title="3. 点位清单" name="points">
				<opcuapointlist v-model="props.modelValue" @submit="forwardLegacySubmit" />
			</el-collapse-item>

			<el-collapse-item title="4. 转换规则" name="transforms">
				<el-form label-width="140px">
					<el-form-item label="表达式">
						<el-input v-model="pointDraft.expression" placeholder="例如 x" />
					</el-form-item>
				</el-form>
			</el-collapse-item>

			<el-collapse-item title="5. 平台映射" name="mapping">
				<el-form label-width="140px">
					<el-form-item label="目标类型">
						<el-select v-model="pointDraft.mapping.targetType">
							<el-option label="遥测" value="Telemetry" />
							<el-option label="属性" value="Attribute" />
						</el-select>
					</el-form-item>
					<el-form-item label="平台属性名">
						<el-input v-model="pointDraft.mapping.targetName" />
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
import opcuapointlist from './opcuapointlist.vue';
import type { OpcUaConnectionModel, OpcUaDeviceModel, OpcUaPointModel } from '../../models/opcua-task';

const props = defineProps({
	modelValue: {
		type: Object,
		default: () => ({}),
	},
});

const emit = defineEmits(['submit']);
const activeSections = ref(['connection', 'devices', 'points', 'mapping']);

const connection = reactive<OpcUaConnectionModel>({
	connectionKey: 'opcua-default',
	connectionName: '默认 OPC UA 连接',
	protocol: 'OpcUa',
	transport: 'Tcp',
	timeoutMs: 3000,
	retryCount: 3,
	protocolOptions: {
		endpointUrl: 'opc.tcp://127.0.0.1:4840',
		securityMode: 'None',
		sessionTimeoutMs: 30000,
	},
});

const device = reactive<OpcUaDeviceModel>({
	deviceKey: 'opcua-device-1',
	deviceName: '示例 OPC UA 设备',
	enabled: true,
	points: [],
	protocolOptions: {
		namespaceIndex: 2,
	},
});

const pointDraft = reactive<OpcUaPointModel>({
	pointKey: 'opcua-point-1',
	pointName: '示例变量',
	sourceType: 'Variable',
	address: 'ns=2;s=Demo.Dynamic.Scalar.Double',
	rawValueType: 'Double',
	length: 1,
	polling: {
		readPeriodMs: 1000,
		group: 'default',
	},
	transforms: [],
	mapping: {
		targetType: 'Telemetry',
		targetName: 'opcUaDemoValue',
		valueType: 'Double',
		displayName: '示例变量',
		group: 'default',
	},
	protocolOptions: {
		nodeId: 'ns=2;s=Demo.Dynamic.Scalar.Double',
		subscriptionMode: 'Subscribe',
		samplingIntervalMs: 1000,
	},
	expression: '',
});

const preview = reactive({
	rawValue: '--',
	transformedValue: '--',
	qualityStatus: 'Good',
	valueType: 'Double',
});

const simulatePreview = () => {
	preview.rawValue = '56.78';
	preview.transformedValue = '56.78';
	preview.qualityStatus = 'Good';
	preview.valueType = pointDraft.mapping.valueType;
};

const forwardLegacySubmit = (payload: any) => {
	emit('submit', payload);
};

const save = () => {
	emit('submit', {
		namespace: 'opcuaworkspacechanged',
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

.protocol-workspace__actions {
	margin-top: 16px;
}
</style>
