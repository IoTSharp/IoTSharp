export interface ExecutorDoc {
	title: string;
	description: string;
	highlights: string[];
	sample: string;
}

export const executorDocs: Record<string, ExecutorDoc> = {
	'IoTSharp.TaskActions.AlarmPullExcutor': {
		title: '告警推送执行器',
		description: '把上游节点的告警内容推送到外部系统，适合对接第三方工单、短信或企业微信告警通道。',
		highlights: [
			'配置中通常只需要目标地址、路径和令牌。',
			'上游节点建议输出标准告警对象，便于下游统一处理。',
			'外部接口尽量保持幂等，避免重复推送带来噪音。',
		],
		sample: `{
  "BaseUrl": "https://example.com",
  "Url": "/alarms/push",
  "Token": "your-access-token"
}`,
	},
	'IoTSharp.TaskActions.PublishAlarmDataTask': {
		title: '告警发布器',
		description: '把脚本或执行器产出的告警对象发布回平台，用于生成正式告警记录。',
		highlights: [
			'建议输出标准告警对象，包含类型、详情和严重级别。',
			'OriginatorName 可以填写设备名称，也可以填写设备 ID。',
			'CreateDateTime 可选，不传时由平台使用当前时间。',
		],
		sample: `{
  "CreateDateTime": "2026-03-21 16:14:00",
  "OriginatorType": "Device",
  "OriginatorName": "Boiler-01",
  "AlarmType": "Overheat",
  "AlarmDetail": "锅炉温度超过阈值",
  "ServerityLevel": "Warning"
}`,
	},
	'IoTSharp.TaskActions.PublishAttributeDataTask': {
		title: '属性发布器',
		description: '向平台发布设备属性，适合保存配置、状态和静态档案类字段。',
		highlights: [
			'建议使用数组形式提交多个属性键值。',
			'每项至少包含 key 和 value 两个字段。',
			'更适合配置状态和相对静态的数据。',
		],
		sample: `[
  {
    "key": "speed",
    "value": "1000"
  },
  {
    "key": "mode",
    "value": "auto"
  }
]`,
	},
	'IoTSharp.TaskActions.PublishTelemetryDataTask': {
		title: '遥测发布器',
		description: '向平台发布设备遥测数据，适合实时上报和时序类字段。',
		highlights: [
			'数据结构和属性发布器一致，都是键值数组。',
			'更适合温度、电流、功率等实时采样数据。',
			'建议在上游节点先做好清洗和单位换算。',
		],
		sample: `[
  {
    "key": "temperature",
    "value": "38.6"
  },
  {
    "key": "pressure",
    "value": "1.2"
  }
]`,
	},
	'IoTSharp.TaskActions.CustomeAlarmPullExcutor': {
		title: '自定义告警组装器',
		description: '把上游输入整理成告警对象，常作为告警发布器的上游准备节点。',
		highlights: [
			'Serverity 必须是整数类型。',
			'AlarmType 用于定义告警分类，建议保持稳定。',
			'AlarmDetail 可以直接拼接设备信息和触发原因。',
		],
		sample: `{
  "Serverity": 7,
  "AlarmType": "Overheat",
  "AlarmDetail": "温度超过 38 度，请及时检查"
}`,
	},
	'IoTSharp.TaskActions.DeviceActionExcutor': {
		title: '设备动作执行器',
		description: '将上游数据 POST 到外部接口，并把执行结果回传给下游节点继续使用。',
		highlights: [
			'配置中填写目标服务地址、接口路径和令牌。',
			'外部接口返回结果中建议保留 success、message 和 result 字段。',
			'适合联动工控平台、边缘服务或企业业务系统。',
		],
		sample: `{
  "BaseUrl": "https://example.com",
  "Url": "/devices/action",
  "Token": "your-access-token"
}`,
	},
	'IoTSharp.TaskActions.MessagePullExecutor': {
		title: '消息推送执行器',
		description: '把上游对象整理后推送到外部消息系统，适合短信、邮件或企业 IM 通道。',
		highlights: [
			'上游 JSON 对象会被整理为键值数组后再推送。',
			'外部接口返回结果建议带 success 字段便于规则继续判断。',
			'推荐在规则里补充明确的失败分支，方便排障。',
		],
		sample: `{
  "BaseUrl": "https://example.com",
  "Url": "/messages/send",
  "Token": "your-access-token"
}`,
	},
	'IoTSharp.TaskActions.RangerCheckExcutor': {
		title: '范围检测执行器',
		description: '用于检测输入数据是否落在预期区间，常用于预警和阈值判断。',
		highlights: [
			'建议在配置中补充上下限和命中后的处理策略。',
			'可以作为告警、属性或消息执行器的前置节点。',
			'当前仓库内说明较少，推荐结合实际规则逐步补充。 ',
		],
		sample: `{
  "Min": 0,
  "Max": 100,
  "Inclusive": true
}`,
	},
	'IoTSharp.TaskActions.TelemetryArrayPullExcutor': {
		title: '遥测数组处理执行器',
		description: '处理批量遥测数组场景，适合对接一次性上报多组测点的设备数据。',
		highlights: [
			'建议在上游保证数组结构稳定，避免字段漂移。',
			'可与遥测发布器串联使用，统一写回平台。',
			'当前说明较少，推荐先用示例结构完成验证。',
		],
		sample: `[
  {
    "key": "temperature",
    "value": "36.8"
  },
  {
    "key": "humidity",
    "value": "65"
  }
]`,
	},
};

export const defaultExecutorDoc: ExecutorDoc = {
	title: '执行器配置',
	description: '当前执行器没有内置专用说明，建议根据处理器的输入输出约定补充 JSON 配置。',
	highlights: [
		'优先确认上游节点会传入什么数据。',
		'保持配置 JSON 结构稳定，便于后续维护。',
		'如果是 HTTP 类执行器，建议明确返回结果的成功标识。',
	],
	sample: `{
  "BaseUrl": "https://example.com",
  "Url": "/handler/path",
  "Token": "optional-token"
}`,
};
