export interface EdgeTableDataRow {
	id?: string;
	name?: string;
	runtimeType?: string;
	runtimeName?: string;
	version?: string;
	status?: string;
	healthy?: boolean | null;
	active?: boolean;
	lastHeartbeatDateTime?: string;
	lastActivityDateTime?: string;
	hostName?: string;
	ipAddress?: string;
	platform?: string;
	capabilities?: string;
	metadata?: string;
	metrics?: string;
}