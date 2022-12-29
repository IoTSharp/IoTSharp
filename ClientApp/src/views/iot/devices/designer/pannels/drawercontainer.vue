<template>
    <div>
        <el-drawer :title="state.title" v-model="state.isOpen" size="90%" @closed="drawerclose">
            <el-scrollbar>

                <modbuspointlist>


                </modbuspointlist>


            </el-scrollbar>
        </el-drawer>
    </div>
</template>

<script lang="ts" setup>
import { reactive, ref, nextTick } from "vue";
import { drawerparams } from "../models/drawerparams";
import modbuspointlist from "./modbus/modbuspointlist.vue";

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