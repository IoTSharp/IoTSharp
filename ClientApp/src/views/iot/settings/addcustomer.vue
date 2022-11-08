<template>
	<div>
		<el-drawer v-model="drawer" :title="dialogtitle" size="50%">
			<div class="add-form-container">
				<el-form :model="dataForm" size="default" label-width="120px">
					<el-row :gutter="35">
						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="客户名称">
								<el-input v-model="dataForm.name" placeholder="请输入客户名称" clearable></el-input>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="客户邮箱地址">
								<el-input v-model="dataForm.eMail" placeholder="请输入客户邮箱" clearable></el-input>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="联系电话">
								<el-input v-model="dataForm.phone" placeholder="请输入客户联系电话" clearable></el-input>
							</el-form-item>
						</el-col>

						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="国家">
								<el-input v-model="dataForm.country" placeholder="请输入客户名称" clearable></el-input>
							</el-form-item>
						</el-col>

						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="省">
								<el-input v-model="dataForm.province" placeholder="请输入客户所在省" clearable></el-input>
							</el-form-item>
						</el-col>

						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="城市">
								<el-input v-model="dataForm.city" placeholder="请输入客户所在城市" clearable></el-input>
							</el-form-item>
						</el-col>

						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="街道">
								<el-input v-model="dataForm.street" placeholder="请输入客户街道" clearable></el-input>
							</el-form-item>
						</el-col>

						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="地址">
								<el-input v-model="dataForm.address" placeholder="请输入客户地址" clearable></el-input>
							</el-form-item>
						</el-col>

						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="邮编">
								<el-input v-model="dataForm.zipCode" placeholder="请输入邮编" clearable></el-input>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item>
								<el-button type="primary" @click="onSubmit">保存</el-button>
								<el-button @click="closeDialog">取消</el-button>
							</el-form-item>
						</el-col>
					</el-row>
				</el-form>
			</div>
		</el-drawer>
	</div>
</template>

<script lang="ts">
import { ref, toRefs, reactive, onMounted, defineComponent, watchEffect } from 'vue';
import { ElMessageBox, ElMessage } from 'element-plus';
import { tenantApi } from '/@/api/tenants';
import { appmessage } from '/@/api/iapiresult';
import { customerApi } from '/@/api/customer';
import { useUserInfo } from '/@/stores/userInfo';
import { storeToRefs } from 'pinia';

interface tenantform {
	drawer: boolean;
	dialogtitle: string;
	dataForm: tenantaddoreditdto;
}

export default defineComponent({
	name: 'addcustomer',
	components: {},
	setup(props, context) {
		const stores = useUserInfo();
		const { userInfos } = storeToRefs(stores);
		const state = reactive<tenantform>({
			dialogtitle: '',
			drawer: false,
			dataForm: {
				id: '00000000-0000-0000-0000-000000000000',
				name: '',
				eMail: '',
				phone: '',
				country: '',
				province: '',
				city: '',
				street: '',
				address: '',
				zipCode: 0,
			},
		});

		const openDialog = (customerId: string) => {
			if (customerId === '00000000-0000-0000-0000-000000000000') {
				state.dataForm = {
					id: '00000000-0000-0000-0000-000000000000',
					name: '',
					eMail: '',
					phone: '',
					country: '',
					province: '',
					city: '',
					street: '',
					address: '',
					zipCode: 0,
				};
				state.dialogtitle = '新增客户';
			} else {
				state.dialogtitle = '修改客户';
				customerApi()
					.getCustomer(customerId)
					.then((res) => {
						state.dataForm = res.data;
					});
			}
			state.drawer = true;
		};
		// 关闭弹窗
		const closeDialog = () => {
			state.drawer = false;
		};

		watchEffect(() => {});

		onMounted(() => {});
		const onSubmit = () => {
			const tenantId = userInfos.value.tenantId.id;
			const data = {
				...state.dataForm,
				tenantId,
			};
			if (state.dataForm.id === '00000000-0000-0000-0000-000000000000') {
				customerApi()
					.postCustomer(data)
					.then((res: appmessage<boolean>) => {
						if (res.code === 10000 && res.data) {
							ElMessage.success('新增成功');
							closeDialog();
							context.emit('getData');
						} else {
							ElMessage.warning('新增失败:' + res.msg);
						}
					});
			} else {
				customerApi()
					.putCustomer(data)
					.then((res: appmessage<boolean>) => {
						if (res.code === 10000 && res.data) {
							ElMessage.success('修改成功');
							closeDialog();
							context.emit('getData');
						} else {
							ElMessage.warning('修改失败:' + res.msg);
						}
					});
			}
		};
		return { ...toRefs(state), onSubmit, openDialog, closeDialog };
	},
});
</script>
