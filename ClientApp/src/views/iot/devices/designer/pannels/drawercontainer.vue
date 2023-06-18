<template>
    <div>
        <el-drawer :title="state.title" v-model="state.isOpen" size="90%" @closed="drawerclose">
            <el-scrollbar>

                <modbuspointlist ref="modbuspointlistRef" @submit="onsubmit"
                    v-if="state.sender?.node?.bizdata?.devnamespace === 'modbus'" v-model="state.sender">


                </modbuspointlist>
                <opcuapointlist @submit="onsubmit" v-if="state.sender?.node?.bizdata?.devnamespace === 'opcua'"
                    v-model="state.sender">


                </opcuapointlist>

                <connector  @submit="onsubmit" v-if="state.sender?.store?.data?.bizdata?.devnamespace === 'connector'">


                </connector>




            </el-scrollbar>
        </el-drawer>
    </div>
</template>

<script lang="ts" setup>
import { reactive, ref, nextTick } from "vue";
import { drawerparams } from "../models/drawerparams";
import modbuspointlist from "./modbus/modbuspointlist.vue";
import opcuapointlist from "./opcua/opcuapointlist.vue";
import connector from "./connector/connector.vue";
const emit = defineEmits(["close", "submit"]);

const modbuspointlistRef = ref();
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