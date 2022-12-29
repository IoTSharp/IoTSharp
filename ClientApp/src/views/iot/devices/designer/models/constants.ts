export const funcodes = new Map(
    [
        ['ReadCoils', { label: '读取线圈', value: 1 }],
        ['ReadDiscreteInputs', { label: '读取离散量输入', value: 2 }],
        ['ReadMultipleHoldingRegisters', { label: '读取保持寄存器', value: 3 }],
        ['ReadInputRegisters', { label: '读取输入寄存器', value: 4 }],
        ['WriteSingleCoil', { label: '写入单个线圈', value: 5 }],
        ['WriteSingleHoldingRegister', { label: '写入单个保持寄存器', value: 6 }],
        ['WriteMultipleCoils', { label: '写入多个线圈', value: 15 }],
        ['WriteMultipleHoldingRegisters', { label: '写入多个保持寄存器', value: 16 }],
    ],
);


export const datatypes = new Map(
    [
        ['Boolean', { label: '逻辑', value: 0 }],
        ['String', { label: '字符串', value: 1 }],
        ['Long', { label: '整数', value: 2 }],
        ['Double', { label: '浮点数', value: 3 }],
        ['DateTime', { label: '时间', value: 4 }],
    ]
);
export const datacatalogs = new Map(
    [
        ['AttributeData', { label: '属性数据', value: 0 }],
        ['TelemetryData', { label: '遥测数据', value: 1 }],

    ]
);
