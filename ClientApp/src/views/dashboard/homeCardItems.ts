import deviceIcon from '~icons/carbon/iot-platform'
import telemetry from '~icons/carbon/ibm-cloud-pak-data'
import event from '~icons/carbon/ibm-cloud-event-streams'
import warning from '~icons/ic/round-warning'
import userIcon from '~icons/ic/baseline-supervisor-account'
import product from '~icons/ic/outline-category'
import rule from '~icons/carbon/flow-modeler'
import assets from '~icons/ic/outline-devices-other'

export const homeCardItemsConfig = [
    {
        zValue: '125,12',
        zKey: '所有设备',
        icon: deviceIcon,
        iconBackgroundColor: '#4945FF',
    },
    {
        zValue: '125,12',
        zKey: '在线',
        icon: deviceIcon,
        iconBackgroundColor: '#10b981',
    },
    {
        zValue: '653,33',
        zKey: '今日属性',
        icon: telemetry,
        iconBackgroundColor: '#002766',
    },
    {
        zValue: '125,65',
        zKey: '今日事件',
        icon: event,
        iconBackgroundColor: '#0050B3',
    },
    {
        zValue: '520,43',
        zKey: '告警设备',
        icon: warning,
        iconBackgroundColor: '#FA9D14',
    },
    {
        zValue: '98',
        zKey: '用户',
        icon: userIcon,
        iconBackgroundColor: '#0091FF',
    },
    {
        zValue: '16',
        zKey: '产品',
        icon: product,
        iconBackgroundColor: '#32C5FF',
    },
    {
        zValue: '9',
        zKey: '规则',
        icon: rule,
        iconBackgroundColor: '#6DD400',
    },
];
