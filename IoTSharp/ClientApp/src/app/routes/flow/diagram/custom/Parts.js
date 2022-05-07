export const
    TASKS = [
        { title: '告警发布器', desc: '告警发布器', namespace: 'PublishAlarmDataTask', color: '#f5222d', shape: 'bpmn:UserTask', action: null, config: {}, classname: 'bpmn-icon-task red', group: 'excutors' },
        { title: '属性发布器', desc: '属性发布器', namespace: 'PublishAttributeDataTask', color: '#fadb14', shape: 'bpmn:ServiceTask', action: null, config: {}, classname: 'bpmn-icon-task yellow', group: 'excutors' },
        { title: '遥测数据发布器', desc: '遥测数据发布器', namespace: 'PublishTelemetryDataTask', color: '#a0d911', shape: 'bpmn:ManualTask', action: null, config: {}, classname: 'bpmn-icon-task green', group: 'excutors' },
        { title: '告警生成器', desc: '自定义的告警生成器', namespace: 'CustomeAlarmPullExcutor', color: '#1890ff', shape: 'bpmn:SendTask', action: null, config: {}, classname: 'bpmn-icon-task blue', group: 'excutors' },
        { title: '告警推送器', desc: '用于告警推送的执行器', namespace: 'AlarmPullExcutor ', color: '#722ed1', shape: 'bpmn:ReceiveTask', action: null, config: {}, classname: 'bpmn-icon-task purple', group: 'excutors' },
        { title: '消息推送器', desc: '用于消息推送的执行器', namespace: 'MessagePullExecutor', color: '#eb2f96', shape: 'bpmn:BusinessRuleTask', action: null, config: {}, classname: 'bpmn-icon-task magenta', group: 'excutors' },
        { title: '遥测数组推器', desc: '用于遥测数组推送的执行器', namespace: 'TelemetryArrayPullExcutor', color: '#13c2c2', shape: 'bpmn:ScriptTask', action: {}, config: null, classname: 'bpmn-icon-task cyan', group: 'excutors' }
    ];	
    