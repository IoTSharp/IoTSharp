import dashboard from '~icons/material-symbols/dashboard-outline-rounded'
import tenantmnt from '~icons/ic/round-account-box'
import customermnt from  '~icons/ic/round-account-circle'
import usermnt from  '~icons/ic/baseline-supervisor-account'
import productmnt from  '~icons/ic/outline-category'
import devicemnt from  '~icons/carbon/iot-platform'
import assetsmnt from  '~icons/ic/outline-devices-other'
import alarmmnt from  '~icons/ic/round-warning'
import rulesmnt from  '~icons/carbon/flow-modeler'
import settingsmnt from  '~icons/carbon/settings'
import { Collection, Connection, Cpu, DataAnalysis, Files, List, Monitor, Postcard, Tickets, Upload, WarningFilled } from '@element-plus/icons-vue'
export const menuIconList = {
    dashboard,
	accessmnt: devicemnt,
	operationsmnt: alarmmnt,
	governancemnt: settingsmnt,
	default: List,
    tenantmnt,
    customermnt,
    usermnt,
    productmnt,
    devicemnt,
    assetsmnt,
    alarmmnt,
    rulesmnt,
    settingsmnt,
	productlist: Collection,
	assetlist: Files,
	devicelist: Monitor,
	gatewaylist: Connection,
	edgelist: Cpu,
	flowlist: rulesmnt,
	flowevents: DataAnalysis,
	edgetasks: Tickets,
	releasecenter: Upload,
	alarmlist: WarningFilled,
	certmnt: Postcard,
	tenantlist: tenantmnt,
	customerlist: customermnt,
	userlist: usermnt,
}
