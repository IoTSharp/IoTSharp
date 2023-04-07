<template>
    <el-form :inline="true" class="demo-form-inline">
        <el-input size="default" placeholder="地图中心点经度" style="max-width: 180px" v-model="state.centerX" v-if="false">
        </el-input>
        <el-input size="default" placeholder="地图中心点维度" style="max-width: 180px" v-model="state.centerY" v-if="false">
        </el-input>
        <el-form-item label="选择经度字段">
            <el-select v-model="state.longitudefield"  placeholder="longitude">
                <el-option v-for="item in state.telemetryKeys" :key="item" :label="item" :value="item" />
            </el-select>
        </el-form-item>
        <el-form-item label="选择纬度字段">
            <el-select v-model="state.latitudefield" placeholder="latitude">
                <el-option v-for="item in state.telemetryKeys" :key="item" :label="item" :value="item" />
            </el-select>
        </el-form-item>
        <el-form-item label="">
            <el-button size="default" type="primary" class="ml10" @click="create()">
                <el-icon>
                    <ele-Search />
                </el-icon>
                查询
            </el-button> </el-form-item>

    </el-form>


    <div class="layout-padding" style="height: 1000px; ">
        <div class="layout-padding-auto layout-padding-view">
            <div ref="echartsMapRef" style="height: 1000px;"></div>
        </div>
    </div>
</template>

<script setup lang="ts" name="funEchartsMap">
import { reactive, onMounted, ref } from 'vue';
import * as echarts from 'echarts';
import 'echarts/extension/bmap/bmap';
import { deviceApi } from '/@/api/devices';
import { appmessage } from '/@/api/iapiresult';


const props = defineProps({
    deviceId: {
        type: String,
        default: ''
    }
})
interface BMapDotData {
    name?: string;
    value?: number[]
    // antherfield:any;
}

interface BMapStateObject {
    echartsMap: HTMLElement | string,
    lines: number[][][];
    dots: BMapDotData[];
    centerX: number;
    centerY: number;
    latitudefield: string;
    longitudefield: string;
    telemetryKeys: string[]
}



interface Point {
    x: number;
    y: number;
    d?: string;
}


interface TelemetryData {
    keyName: string;
    dateTime: string;
    dataType: String;
    value: number;
}

// const latitudefield = "cell_latitude"
// const longitudefield = "cell_longitude"


// const latitudeSampleData: TelemetryData[] = [

//     { keyName: latitudefield, dateTime: '2018-5-3', dataType: 'string', value: 43.69228888604795 },
//     { keyName: latitudefield, dateTime: '2018-6-3', dataType: 'string', value: 43.68228888604795 },
//     { keyName: latitudefield, dateTime: '2018-7-3', dataType: 'string', value: 43.67228888604795 },
//     { keyName: latitudefield, dateTime: '2018-8-3', dataType: 'string', value: 43.66228888604795 },


// ];

// const longitudeSampleData: TelemetryData[] = [
//     { keyName: longitudefield, dateTime: '2018-5-3', dataType: 'string', value: 87.6503034374998 },
//     { keyName: longitudefield, dateTime: '2018-6-3', dataType: 'string', value: 87.6403034374998 },
//     { keyName: longitudefield, dateTime: '2018-7-3', dataType: 'string', value: 87.6303034374998 },
//     { keyName: longitudefield, dateTime: '2018-8-3', dataType: 'string', value: 87.6203034374998 },
// ];


const x_PI = 3.141592653589793 * 3000.0 / 180.0
const PI = 3.141592653589793
const a = 6378245.0
const ee: number = 0.006693421622965943
const echartsMapRef = ref<HTMLElement>();

const state = reactive<BMapStateObject>({
    echartsMap: '',
    lines: [],
    dots: [],
    centerX: 87.6403034374998,
    centerY: 43.68228888604795, latitudefield: 'cell_latitude', longitudefield: 'cell_longitude', telemetryKeys: []

});

const combineandtranslategeodata = (longitudedata: TelemetryData[], latitudedata: TelemetryData[]): Point[] => {


    var geodata: Point[] = [];
    for (let longitude of longitudedata) {
        var latitude = latitudedata.find(x => x.dateTime == longitude.dateTime)
        if (latitude) {
            var data = wgs84tobd09(Number(longitude.value), Number(latitude.value));
            geodata.push({ x: data[0], y: data[1], d: latitude.dateTime });
        }
    }
    return geodata
}

const computercenter = (points: Point[], epsilon: number, maxIterations: number): Point => {
    let center: Point = { x: 0, y: 0 };
    let lastCenter: Point;

    for (let i = 0; i < maxIterations; i++) {
        let denominator = 0;
        let numeratorX = 0;
        let numeratorY = 0;

        for (let point of points) {
            const distance = Math.sqrt((point.x - center.x) ** 2 + (point.y - center.y) ** 2);
            if (distance < epsilon) {
                return center;
            }
            denominator += 1 / distance;
            numeratorX += point.x / distance;
            numeratorY += point.y / distance;
        }

        lastCenter = center;
        center = { x: numeratorX / denominator, y: numeratorY / denominator };

        if (lastCenter && Math.abs(center.x - lastCenter.x) < epsilon && Math.abs(center.y - lastCenter.y) < epsilon) {
            return center;
        }
    }

    return center;

}
// 初始化 echartsMap
const initEchartsMap = (centerX: number, centerY: number) => {
    const myChart = echarts.init(echartsMapRef.value!);


    const option = {
        tooltip: {
            trigger: 'item',
        },
        color: ['#9a60b4', '#ea7ccc'],
        bmap: {
            center: [centerX, centerY],
            zoom: 10,
            roam: true,
            mapStyle: {},
        },
        series: [
            {
                name: 'lines',
                type: 'lines',
                coordinateSystem: 'bmap',
                data: state.lines,
                polyline: true,
                lineStyle: {
                    color: 'red',
                    opacity: 0.6,
                    width: 5
                }
            },

            {
                name: '坐标',
                type: 'effectScatter',
                coordinateSystem: 'bmap',
                data: state.dots,
                symbolSize: 10,
                encode: {
                    value: 2,
                },
                showEffectOn: 'render',
                rippleEffect: {
                    brushType: 'stroke',
                },
                hoverAnimation: true,
                label: {
                    formatter: x => {
                        return x.name;
                    },
                    position: 'right',
                    show: true,
                },
                itemStyle: {
                    shadowBlur: 10,
                    shadowColor: '#333',
                },
                zlevel: 1,
            },

        ],
    };
    myChart.setOption(option);

    window.addEventListener('resize', () => {
        myChart.resize();
    });
};


const create = async () => {
    var res = await deviceApi().getDeviceTelemetryLatestByKeys(props.deviceId, state.latitudefield + ',' + state.longitudefield)
    var result = res as unknown as appmessage<TelemetryData[]>
    var geodata = combineandtranslategeodata(result.data?.filter(x => x.keyName === state.longitudefield) ?? [], result.data?.filter(x => x.keyName == state.latitudefield) ?? []);

    // var center = computercenter(geodata, 1, geodata.length);

    if (geodata.length > 0) {



        //    var points=[];
        //     state.lines[0] = [];
        //     var points = [];
        //     geodata.forEach(d => {
        //         points.push(new BMap.Point(d.x, d.y))
        //     })
        //     var convertor = new BMap.Convertor();
        //   百度官方坐标转换,校准坐标偏移
        //     convertor.translate(points, 1, 5, (x: any) => {


        //         x.points.forEach(c => {

        //             state.dots.push({ name: c.lng, value: [c.lng, c.lat] })
        //             state.lines[0].push([c.lng, c.lat]);
        //             if (geodata.length == 1) {
        //                 state.lines[0].push([c.lng, c.lat]);
        //             }

        //         })
        //         var center = computercenter(geodata, 1, geodata.length);
        //         state.centerX = center.x;
        //         state.centerY = center.y;        initEchartsMap(center.x, center.y);
        //     })
        state.lines[0] = [];
        geodata.forEach(c => {
            state.dots.push({ name: c.d, value: [c.x, c.y] })
            state.lines[0].push([c.x, c.y]);
            if (geodata.length == 1) {
                state.lines[0].push([c.x, c.y]);
            }

        })
        var center = computercenter(geodata, 1, geodata.length);
        state.centerX = center.x;
        state.centerY = center.y;
        initEchartsMap(state.centerX, state.centerY);

    } else {

        initEchartsMap(state.centerX, state.centerY);
    }

}

const getData = async (deviceid: string) => {
    const res = await deviceApi().getDeviceLatestTelemetry(deviceid);
    state.telemetryKeys = res.data.map((c) => c.keyName);
}
watch(() => props.deviceId, async () => {
    await getData(props.deviceId)
})



const bd09togcj02 = (bd_lon: number, bd_lat: number) => {

    var x = bd_lon - 0.0065
    var y = bd_lat - 0.006
    var z = Math.sqrt(x * x + y * y) - 0.00002 * Math.sin(y * x_PI)
    var theta = Math.atan2(y, x) - 0.000003 * Math.cos(x * x_PI)
    var gg_lng = z * Math.cos(theta)
    var gg_lat = z * Math.sin(theta)
    return [gg_lng, gg_lat]


}


const gcj02tobd09 = (lng: number, lat: number) => {
    var z = Math.sqrt(lng * lng + lat * lat) + 0.00002 * Math.sin(lat * x_PI)
    var theta = Math.atan2(lat, lng) + 0.000003 * Math.cos(lng * x_PI)
    var bd_lng = z * Math.cos(theta) + 0.0065
    var bd_lat = z * Math.sin(theta) + 0.006
    return [bd_lng, bd_lat]

}


const wgs84togcj02 = (lng: number, lat: number) => {

    if (outOfChina(lng, lat)) {
        return [lng, lat]
    }
    else {
        var dlat = transformlat(lng - 105.0, lat - 35.0)
        var dlng = transformlng(lng - 105.0, lat - 35.0)
        var radlat = lat / 180.0 * PI
        var magic = Math.sin(radlat)
        magic = 1 - ee * magic * magic
        var sqrtmagic = Math.sqrt(magic)
        dlat = (dlat * 180.0) / ((a * (1 - ee)) / (magic * sqrtmagic) * PI)
        dlng = (dlng * 180.0) / (a / sqrtmagic * Math.cos(radlat) * PI)
        const mglat = lat + dlat
        const mglng = lng + dlng
        return [mglng, mglat]
    }

}

const gcj02towgs84 = (lng: number, lat: number) => {
    if (outOfChina(lng, lat)) {
        return [lng, lat]
    }
    else {
        var dlat = transformlat(lng - 105.0, lat - 35.0)
        var dlng = transformlng(lng - 105.0, lat - 35.0)
        var radlat = lat / 180.0 * PI
        var magic = Math.sin(radlat)
        magic = 1 - ee * magic * magic
        var sqrtmagic = Math.sqrt(magic)
        dlat = (dlat * 180.0) / ((a * (1 - ee)) / (magic * sqrtmagic) * PI)
        dlng = (dlng * 180.0) / (a / sqrtmagic * Math.cos(radlat) * PI)
        const mglat = lat + dlat
        const mglng = lng + dlng
        return [lng * 2 - mglng, lat * 2 - mglat]
    }


}


const bd09towgs84 = (lng: number, lat: number) => {

    const gcj02 = bd09togcj02(lng, lat)
    // 火星坐标系转wgs84坐标系
    const result = gcj02towgs84(gcj02[0], gcj02[1])
    return result

}

const transformlat = (lng: number, lat: number) => {
    var ret = -100.0 + 2.0 * lng + 3.0 * lat + 0.2 * lat * lat + 0.1 * lng * lat + 0.2 * Math.sqrt(Math.abs(lng))
    ret += (20.0 * Math.sin(6.0 * lng * PI) + 20.0 * Math.sin(2.0 * lng * PI)) * 2.0 / 3.0
    ret += (20.0 * Math.sin(lat * PI) + 40.0 * Math.sin(lat / 3.0 * PI)) * 2.0 / 3.0
    ret += (160.0 * Math.sin(lat / 12.0 * PI) + 320 * Math.sin(lat * PI / 30.0)) * 2.0 / 3.0
    return ret
}

const transformlng = (lng: number, lat: number) => {

    var ret = 300.0 + lng + 2.0 * lat + 0.1 * lng * lng + 0.1 * lng * lat + 0.1 * Math.sqrt(Math.abs(lng))
    ret += (20.0 * Math.sin(6.0 * lng * PI) + 20.0 * Math.sin(2.0 * lng * PI)) * 2.0 / 3.0
    ret += (20.0 * Math.sin(lng * PI) + 40.0 * Math.sin(lng / 3.0 * PI)) * 2.0 / 3.0
    ret += (150.0 * Math.sin(lng / 12.0 * PI) + 300.0 * Math.sin(lng / 30.0 * PI)) * 2.0 / 3.0
    return ret

}

const wgs84tobd09 = (lng: number, lat: number) => {
    const gcj02 = wgs84togcj02(lng, lat)
    // 火星坐标系转百度坐标系
    const result = gcj02tobd09(gcj02[0], gcj02[1])
    return result



}


const outOfChina = (lng: number, lat: number): boolean => {
    return (lng < 72.004 || lng > 137.8347) || ((lat < 0.8293 || lat > 55.8271) || false)
}

// 页面加载时
onMounted(() => {
    getData(props.deviceId)




    // var geodata = combinegeodata(longitudeSampleData, latitudeSampleData);


    // state.lines[0] = [];
    // geodata.forEach(c => {
    //     state.lines[0].push([c.x, c.y]);
    //     state.dots.push({ name: c.d, value: [c.x, c.y] })
    // })


    // var center = computercenter(geodata, 1, geodata.length);
    // state.centerX = center.x;
    // state.centerY = center.y;
    // initEchartsMap(center.x, center.y);


});
</script>