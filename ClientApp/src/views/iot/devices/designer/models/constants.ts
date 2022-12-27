export const funcode = new Map(
    [
        ['ReadCoils', { lebel: '读取线圈', value: 1 }],
        ['ReadDiscreteInputs', { lebel: '读取离散量输入', value: 2 }],
        ['ReadMultipleHoldingRegisters', { lebel: '读取保持寄存器', value: 3 }],
        ['ReadInputRegisters', { lebel: '读取输入寄存器', value: 4 }],
        ['WriteSingleCoil', { lebel: '写入单个线圈', value: 5 }],
        ['WriteSingleHoldingRegister', { lebel: '写入单个保持寄存器', value: 6 }],
        ['WriteMultipleCoils', { lebel: '写入多个线圈', value: 15 }],
        ['WriteMultipleHoldingRegisters', { lebel: '写入多个保持寄存器', value: 16 }],
    ],
);


export const datatypes = new Map(
    [
        ['Boolean', { lebel: '逻辑', value: 0 }],
        ['String', { lebel: '字符串', value: 1 }],
        ['Long', { lebel: '整数', value: 2 }],
        ['Double', { lebel: '浮点数', value: 3 }],
        ['DateTime', { lebel: '时间', value: 4 }],
    ]
);
export const datacatalogs = new Map(
    [
        ['AttributeData', { lebel: '属性数据', value: 0 }],
        ['TelemetryData', { lebel: '遥测数据', value: 1 }],

    ]
);
