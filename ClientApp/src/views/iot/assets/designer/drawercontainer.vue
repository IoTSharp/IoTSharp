<template>
    <div>
        <el-drawer :title="state.title" v-model="state.isOpen" size="90%" @closed="drawerclose">
            <el-scrollbar>
                <devicepanel></devicepanel>
                <assetpanel></assetpanel>
                <mapping></mapping>
            </el-scrollbar>
        </el-drawer>
    </div>
</template>

<script lang="ts" setup>
import { number } from "@intlify/core-base";
import { reactive, ref, nextTick } from "vue";
import devicepanel from "./panels/devicepanel.vue";
import assetpanel from "./panels/assetpanel.vue";
import mapping from "./panels/mapping.vue";
const emit = defineEmits(["close", "submit"]);

const modbuspointlistRef = ref();
interface DrawerState {

    width: string;
    title: string;
    isOpen: boolean;
    sender: any;
}

interface drawerparams {
    width: string;
    title: string
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
    // switch (state.sender.node.bizdata.devnamespace) {
    //     case 'modbus':
    //         nextTick(()=>{
    //             modbuspointlistRef?.value.loaddata()
    //         })

    //         break;
    //     case 'opcua':
    //         break;


    // }


}
defineExpose({
    open,
});
</script>