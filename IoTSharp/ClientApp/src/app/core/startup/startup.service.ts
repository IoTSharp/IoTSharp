import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ACLService } from '@delon/acl';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';
import { ALAIN_I18N_TOKEN, MenuService, SettingsService, TitleService } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzIconService } from 'ng-zorro-antd/icon';
import { concat, Observable, zip } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { ICONS } from '../../../style-icons';
import { ICONS_AUTO } from '../../../style-icons-auto';
import { I18NService } from '../i18n/i18n.service';

/**
 * Used for application startup
 * Generally used to get the basic data of the application, like: Menu Data, User Data, etc.
 */
@Injectable()
export class StartupService {
  constructor(
    iconSrv: NzIconService,
    private menuService: MenuService,

    @Inject(ALAIN_I18N_TOKEN) private i18n: I18NService,
    private translate: TranslateService,
    private settingService: SettingsService,
    private aclService: ACLService,
    private titleService: TitleService,
    private httpClient: HttpClient,
    private router: Router,
    @Inject(DA_SERVICE_TOKEN) private tokenService: ITokenService
  ) {
    iconSrv.addIcon(...ICONS_AUTO, ...ICONS);
  }

  load(): Observable<void> {
    const defaultLang = this.i18n.defaultLang;
    var token = this.tokenService;
    if (token && token.get() && token.get()?.token) {
      return concat(
        this.httpClient.get(`api/i18n/current?_allow_anonymous=true&lang=${this.i18n.defaultLang}`).pipe(
          map((langData: any) => {
            this.translate.setTranslation(this.i18n.defaultLang, langData.data);
            this.translate.setDefaultLang(this.i18n.defaultLang);
          })
        ),
        this.httpClient.get('api/Menu/GetProfile').pipe(
          map(appData => {
            const res = appData as NzSafeAny;

            this.settingService.setApp({ name: 'IoTSharp', description: 'IoTSharp' });
            this.settingService.setData('drawerconfig', { width: 720, nzMaskClosable: false });
            this.settingService.setUser({
              name: res.data.username,
              avatar: '',
              email: res.data.email,
              modules: res.data.modules,
              comstomer: res.data.comstomer,
              tenant: res.data.tenant
            });
            var ACL = [];
            if (!res.data.funcs) {
              for (var i = 0; i < 500; i++) {
                ACL = [...ACL, i];
              }
            } else {
              this.aclService.setAbility(res.data.funcs);
            }
            //this.aclService.setAbility(ACL);
            this.aclService.setFull(false); //开启ACL

            if (!res.data.menu) {
              //   res.data.menu = menu;
            }
            this.menuService.add(res.data.menu);
            this.titleService.default = '';
            this.titleService.suffix = res.data.appName;
          })
        )
      );
    } else {
      return concat(
        this.httpClient.get(`api/i18n/current?_allow_anonymous=true&lang=${this.i18n.defaultLang}`).pipe(
          map((langData: any) => {
            this.translate.setTranslation(this.i18n.defaultLang, langData.data);
            this.translate.setDefaultLang(this.i18n.defaultLang);
          })
        )
      );
    }
  }
}
