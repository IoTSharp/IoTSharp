import request from '/@/utils/request';

export type DashboardTrendRange = '24h' | '7d';

export interface DashboardTrendPoint {
    startUtc: string;
    received: number | null;
    processed: number | null;
    telemetry: number | null;
    attributes: number | null;
    onlineDevices: number | null;
    partial: boolean;
}

export interface DashboardTrend {
    range: DashboardTrendRange;
    bucketMinutes: 60 | 360;
    state: 'ready' | 'empty' | 'collecting';
    points: DashboardTrendPoint[];
}

export function getKanban () {
    return request({
        url: '/api/home/kanban',
        method: 'get',
    })
}
export function getHealthChecks() {
    return request({
        url: '/api/healthChecks',
        method: 'get',
    })
}

export function getMessageInfo() {
    return request({
        url: '/api/Metrics/EventBus',
        method: 'get',
    })
}

export async function getDashboardTrend(range: DashboardTrendRange): Promise<DashboardTrend> {
    const response = await request<DashboardTrend, { data: DashboardTrend }>({
        url: '/api/DashBoard/Trend',
        method: 'get',
        params: { range },
        timeout: 12000,
    });
    return response.data;
}
