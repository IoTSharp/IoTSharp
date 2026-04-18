<template>
    <div>
        <el-drawer :title="state.title" v-model="state.isOpen" size="90%" @closed="drawerclose">
            <el-scrollbar>
                <modbusworkspace
                    v-if="state.sender?.node?.bizdata?.devnamespace === 'modbus'"
                    v-model="state.sender"
                    @submit="onsubmit"
                />
                <opcuaworkspace
                    v-else-if="state.sender?.node?.bizdata?.devnamespace === 'opcua'"
                    v-model="state.sender"
                    @submit="onsubmit"
                />
                <connector @submit="onsubmit" v-else-if="state.sender?.store?.data?.bizdata?.devnamespace === 'connector'" />
            </el-scrollbar>
        </el-drawer>
    </div>
</template>

<script lang="ts" setup>
import { reactive } from "vue";
import { drawerparams } from "../models/drawerparams";
import modbusworkspace from "./modbus/modbus.vue";
import opcuaworkspace from "./opcua/opcua.vue";
import connector from "./connector/connector.vue";
const emit = defineEmits(["close", "submit"]);
interface DrawerState {

    width: string;
    title: string;
    isOpen: boolean;
    sender: any;
}
const state = reactive<DrawerState>({
    width: '50%',
    isOpen: false,
    title: '',
    sender: {}
});

const drawerclose = () => {

};

const onsubmit = (param: any) => {
    emit("submit", param);
}


const open = (sender: any, params: drawerparams) => {
    if (params) {
        state.width = params.width ?? '50%'
        state.title = params.title ?? ''
    }

    state.isOpen = true;
    state.sender = sender;
}
defineExpose({
    open,
});
</script>