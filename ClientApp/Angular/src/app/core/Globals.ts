
import { Injectable } from '@angular/core';
import { CacheService } from '@delon/cache';
@Injectable()
export class Globals {
    drawerwidth: number = 720;
    nzMaskClosable: boolean = true;
    UserWigets: string[] = [];

    constructor(public srv: CacheService) {
        this.srv.get<UserProfile>('userProfile').subscribe((x) => {
            this.drawerwidth = x.drawerwidth;
            this.nzMaskClosable = x.nzMaskClosable;
            this.UserWigets = x.UserWigets;
        });
    }
    public load() { }
}

export interface UserProfile {
    drawerwidth: number;
    nzMaskClosable: boolean;
    UserWigets: string[];
}