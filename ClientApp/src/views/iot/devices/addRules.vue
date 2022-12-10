<template>
    <div>
        <el-drawer v-model="state.drawer" :title="state.dialogtitle" size="70%">
            <el-form :model="state.dataForm" label-width="120px">
                <el-form-item label="请选择规则">
                    <el-select v-model="state.dataForm.rule" placeholder="Select">
                        <el-option v-for="item in state.rules" :key="item.value" :label="item.label" :value="item.value"
                            :disabled="item.disabled" />
                    </el-select>
                </el-form-item>
                <el-form-item label="请选择设备">
                    <el-checkbox-group v-model="state.dataForm.dev">
                        <el-checkbox v-for="device in state.devices" :label="device.id" :checked="true">{{ device.name
                        }}</el-checkbox>
                    </el-checkbox-group>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" @click="onSubmit">下发</el-button>
                    <el-button @click="closeDialog">取消</el-button>
                </el-form-item>
            </el-form>
        </el-drawer>
    </div>
</template>

<script lang="ts" setup>
import { ruleApi } from "/@/api/flows";
interface addruleform {
    drawer: boolean;
    dialogtitle: string;

    devices: Array<any>;
    rules: Array<any>;
    dataForm: addruledto;

}
interface addruledto {
    rule: string;
    dev: any;
}

const state = reactive<addruleform>({
    drawer: false,
    dialogtitle: "规则下发",
    devices: [
    ],
    rules: [],
    dataForm: {
        rule: "",
        dev: []
    }
});

const emit = defineEmits(["close", "submit"]);
const openDialog = (devices: Array<any>) => {
    //  emit("submit",state.devices);  
    state.drawer = true;
    state.devices = [...devices];
};
// 关闭弹窗
const closeDialog = () => {
    state.drawer = false;
    emit("close", state.devices);
};

const onSubmit = async () => {
    var result = await ruleApi().bindDevice(state.dataForm);
    if (result["code"] === 10000) {
        ElMessage.success("下发成功");
        state.drawer = false;
        emit("close", { sender: "deviceform", args: state.dataForm });
    } else {
        ElMessage.warning("下发失败:" + result["msg"]);
        emit("close", state.dataForm);
    }
}

onMounted(() => {
    ruleApi()
        .ruleList({
            limit: 100, offset: 0
        })
        .then((res) => {
            state.rules = [...res.data.rows.map(c => { return { value: c.ruleId, label: c.name } })]
        });

});
const tagclose = (tag: any) => {
    state.devices = state.devices.filter(c => c.id != tag.id);
}


defineExpose({
    openDialog,
});
</script>
