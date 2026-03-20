<template>
  <div>
    <el-drawer
      v-model="state.drawer"
      :title="state.dialogTitle"
      size="100%"
    >
      <el-alert
        type="info"
        show-icon
        :closable="false"
        style="margin-bottom: 16px;"
        description="将产品的抽象字段（左侧）与具体设备的字段（右侧）进行关联映射。上传遥测/属性数据时，系统会自动按此映射将数据路由到对应设备字段。"
      />

      <el-row :gutter="16" style="margin-bottom: 12px;">
        <el-col :span="24">
          <el-button type="primary" @click="addRow" :icon="Plus">新增映射</el-button>
          <el-button type="success" @click="save" :icon="Check">保存</el-button>
        </el-col>
      </el-row>

      <el-table :data="state.mappings" border stripe style="width: 100%">
        <!-- 产品字段 -->
        <el-table-column label="产品字段名" min-width="160">
          <template #default="scope">
            <el-select
              v-model="scope.row.produceKeyName"
              placeholder="选择产品字段"
              clearable
              style="width: 100%"
            >
              <el-option
                v-for="key in state.produceKeys"
                :key="key.keyName"
                :label="key.keyName"
                :value="key.keyName"
              >
                <span>{{ key.keyName }}</span>
                <el-tag size="small" style="margin-left: 8px;" :type="key.dataSide === 'ClientSide' ? 'success' : 'info'">
                  {{ key.dataSide }}
                </el-tag>
              </el-option>
            </el-select>
          </template>
        </el-table-column>

        <!-- 数据类型 -->
        <el-table-column label="数据类别" min-width="150">
          <template #default="scope">
            <el-select
              v-model="scope.row.dataCatalog"
              placeholder="选择数据类别"
              style="width: 100%"
            >
              <el-option label="遥测数据 (TelemetryData)" value="TelemetryData" />
              <el-option label="属性数据 (AttributeData)" value="AttributeData" />
            </el-select>
          </template>
        </el-table-column>

        <!-- 映射设备 -->
        <el-table-column label="关联设备" min-width="180">
          <template #default="scope">
            <el-select
              v-model="scope.row.deviceId"
              placeholder="选择设备"
              clearable
              style="width: 100%"
              @change="onDeviceChange(scope.row)"
            >
              <el-option
                v-for="dev in state.devices"
                :key="dev.id"
                :label="dev.name"
                :value="dev.id"
              />
            </el-select>
          </template>
        </el-table-column>

        <!-- 设备字段名 -->
        <el-table-column label="设备字段名" min-width="180">
          <template #default="scope">
            <el-select
              v-if="getDeviceKeys(scope.row.deviceId, scope.row.dataCatalog).length > 0"
              v-model="scope.row.deviceKeyName"
              placeholder="选择或输入设备字段"
              filterable
              allow-create
              clearable
              style="width: 100%"
            >
              <el-option
                v-for="key in getDeviceKeys(scope.row.deviceId, scope.row.dataCatalog)"
                :key="key"
                :label="key"
                :value="key"
              />
            </el-select>
            <el-input
              v-else
              v-model="scope.row.deviceKeyName"
              placeholder="输入设备字段名"
              clearable
            />
          </template>
        </el-table-column>

        <!-- 描述 -->
        <el-table-column label="描述" min-width="150">
          <template #default="scope">
            <el-input v-model="scope.row.description" placeholder="可选备注" clearable />
          </template>
        </el-table-column>

        <!-- 操作 -->
        <el-table-column label="操作" width="80" fixed="right">
          <template #default="scope">
            <el-button
              type="danger"
              size="small"
              text
              :icon="Delete"
              @click="removeRow(scope.$index)"
            >删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-empty v-if="state.mappings.length === 0" description="暂无映射，点击「新增映射」添加" />
    </el-drawer>
  </div>
</template>

<script lang="ts" setup>
import { reactive } from 'vue';
import { ElMessage } from 'element-plus';
import { Plus, Check, Delete } from '@element-plus/icons-vue';
import { v4 as uuidv4, NIL as NIL_UUID } from 'uuid';
import {
  getProduceData,
  getProduceDataMappings,
  saveProduceDataMappings,
  ProduceDataMappingDto,
} from '/@/api/produce';
import { deviceApi } from '/@/api/devices';

const deviceApis = deviceApi();

interface MappingRow extends ProduceDataMappingDto {
  _rowId: string;
}

interface ProduceKey {
  keyName: string;
  dataSide: string;
  type: string;
}

interface DeviceInfo {
  id: string;
  name: string;
}

interface State {
  drawer: boolean;
  dialogTitle: string;
  produceId: string;
  produceKeys: ProduceKey[];
  devices: DeviceInfo[];
  /** deviceId -> { telemetry: string[], attribute: string[] } */
  deviceKeysCache: Record<string, { telemetry: string[]; attribute: string[] }>;
  mappings: MappingRow[];
}

const emit = defineEmits(['close', 'submit']);

const state = reactive<State>({
  drawer: false,
  dialogTitle: '产品数据映射设计器',
  produceId: NIL_UUID,
  produceKeys: [],
  devices: [],
  deviceKeysCache: {},
  mappings: [],
});

const openDialog = async (produceId: string, devices: DeviceInfo[]) => {
  state.produceId = produceId;
  state.devices = devices ?? [];
  state.deviceKeysCache = {};

  // Load produce abstract keys
  try {
    const res = await getProduceData(produceId);
    state.produceKeys = (res.data ?? []).map((x: any) => ({
      keyName: x.keyName,
      dataSide: x.dataSide,
      type: x.type,
    }));
  } catch {
    state.produceKeys = [];
  }

  // Load existing mappings
  try {
    const res = await getProduceDataMappings(produceId);
    state.mappings = (res.data ?? []).map((m: ProduceDataMappingDto) => ({
      _rowId: uuidv4(),
      id: m.id,
      produceKeyName: m.produceKeyName,
      dataCatalog: m.dataCatalog,
      deviceId: m.deviceId,
      deviceKeyName: m.deviceKeyName,
      description: m.description ?? '',
    }));
    // Pre-load device keys for devices already referenced
    const usedDeviceIds = [...new Set(state.mappings.map((m) => m.deviceId).filter(Boolean))];
    for (const devId of usedDeviceIds) {
      await loadDeviceKeys(devId);
    }
  } catch {
    state.mappings = [];
  }

  state.drawer = true;
};

const closeDialog = () => {
  state.drawer = false;
};

const addRow = () => {
  state.mappings.push({
    _rowId: uuidv4(),
    id: NIL_UUID,
    produceKeyName: '',
    dataCatalog: 'TelemetryData',
    deviceId: '',
    deviceKeyName: '',
    description: '',
  });
};

const removeRow = (index: number) => {
  state.mappings.splice(index, 1);
};

const onDeviceChange = async (row: MappingRow) => {
  if (row.deviceId) {
    await loadDeviceKeys(row.deviceId);
    row.deviceKeyName = '';
  }
};

const loadDeviceKeys = async (deviceId: string) => {
  if (state.deviceKeysCache[deviceId]) return;
  try {
    const [attrRes, telemRes] = await Promise.all([
      deviceApis.getDeviceAttributes(deviceId),
      deviceApis.getDeviceLatestTelemetry(deviceId),
    ]);
    const attrKeys = (attrRes.data ?? []).map((a: any) => a.keyName as string).filter(Boolean);
    const telemKeys = (telemRes.data ?? []).map((t: any) => t.keyName as string).filter(Boolean);
    state.deviceKeysCache[deviceId] = { attribute: attrKeys, telemetry: telemKeys };
  } catch {
    state.deviceKeysCache[deviceId] = { attribute: [], telemetry: [] };
  }
};

const getDeviceKeys = (deviceId: string, catalog: string): string[] => {
  if (!deviceId || !state.deviceKeysCache[deviceId]) return [];
  if (catalog === 'AttributeData') return state.deviceKeysCache[deviceId].attribute;
  if (catalog === 'TelemetryData') return state.deviceKeysCache[deviceId].telemetry;
  return [];
};

const save = async () => {
  // Validate
  for (const row of state.mappings) {
    if (!row.produceKeyName || !row.dataCatalog || !row.deviceId || !row.deviceKeyName) {
      ElMessage.warning('请填写所有必填字段（产品字段、数据类别、关联设备、设备字段）');
      return;
    }
  }

  const payload = {
    produceId: state.produceId,
    mappings: state.mappings.map((m) => ({
      id: m.id,
      produceKeyName: m.produceKeyName,
      dataCatalog: m.dataCatalog,  // send enum name string directly; Newtonsoft.Json parses by name
      deviceId: m.deviceId,
      deviceKeyName: m.deviceKeyName,
      description: m.description ?? '',
    })),
  };

  try {
    const result = await saveProduceDataMappings(payload as any);
    if (result['code'] === 10000) {
      ElMessage.success('保存成功');
      state.drawer = false;
      emit('submit', payload);
    } else {
      ElMessage.warning('保存失败：' + result['msg']);
    }
  } catch (e: any) {
    ElMessage.error('保存出错：' + (e.message ?? e));
  }
};

defineExpose({ openDialog, closeDialog });
</script>
