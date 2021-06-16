import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { ACLService } from '@delon/acl';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';
import { ALAIN_I18N_TOKEN, MenuService, SettingsService, TitleService } from '@delon/theme';
import { TranslateService } from '@ngx-translate/core';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzIconService } from 'ng-zorro-antd/icon';
import { concat, zip } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ICONS } from '../../../style-icons';
import { ICONS_AUTO } from '../../../style-icons-auto';
import { I18NService } from '../i18n/i18n.service';

/**
 * 用于应用启动时
 * 一般用来获取应用所需要的基础数据等
 */
@Injectable()
export class StartupService {
  constructor(
    iconSrv: NzIconService,
    private menuService: MenuService,
    private translate: TranslateService,
    @Inject(ALAIN_I18N_TOKEN) private i18n: I18NService,
    private settingService: SettingsService,
    private aclService: ACLService,
    private titleService: TitleService,
    private httpClient: HttpClient, @Inject(DA_SERVICE_TOKEN) private tokenService: ITokenService,
  ) {
    iconSrv.addIcon(...ICONS_AUTO, ...ICONS);
  }

  load(): Promise<void> {
    // only works with promises
    // https://github.com/angular/angular/issues/15088

    var token = this.tokenService;

    if (token && token.get() && token.get()?.token) {

      return concat(this.httpClient.get(`assets/tmp/i18n/${this.i18n.defaultLang}.json`).pipe(map(langData => {
        this.translate.setTranslation(this.i18n.defaultLang, langData);
        this.translate.setDefaultLang(this.i18n.defaultLang);


      })), this.httpClient.get('api/Account/MyInfo?_allow_anonymous=true').pipe(map(appData => {
        const res = appData as NzSafeAny;

        this.settingService.setApp(res.app);

        this.settingService.setUser(res.user);

        this.aclService.setFull(true);

        this.menuService.add(res.menu);

        this.titleService.default = '';
        this.titleService.suffix = res.app.name;

      }))).toPromise()

    } else {
      return concat(this.httpClient.get(`assets/tmp/i18n/${this.i18n.defaultLang}.json`).pipe(map(langData => {
        this.translate.setTranslation(this.i18n.defaultLang, langData);
        this.translate.setDefaultLang(this.i18n.defaultLang);
      }))).toPromise()

    }





    // return new Promise((resolve) => {
    //   zip(this.httpClient.get(`assets/tmp/i18n/${this.i18n.defaultLang}.json`), this.httpClient.get('assets/tmp/app-data.json'))
    //     .pipe(
    //       // 接收其他拦截器后产生的异常消息
    //       catchError((res) => {
    //         console.warn(`StartupService.load: Network request failed`, res);
    //         resolve();
    //         return [];
    //       }),
    //     )
    //     .subscribe(
    //       ([langData, appData]) => {
    //         // setting language data
    //         this.translate.setTranslation(this.i18n.defaultLang, langData);
    //         this.translate.setDefaultLang(this.i18n.defaultLang);

    //         // application data
    //         const res = appData as NzSafeAny;
    //         // 应用信息：包括站点名、描述、年份
    //         this.settingService.setApp(res.app);
    //         // 用户信息：包括姓名、头像、邮箱地址
    //         this.settingService.setUser(res.user);
    //         // ACL：设置权限为全量
    //         this.aclService.setFull(true);
    //         // 初始化菜单
    //         this.menuService.add(res.menu);
    //         // 设置页面标题的后缀
    //         this.titleService.default = '';
    //         this.titleService.suffix = res.app.name;
    //       },
    //       () => { },
    //       () => {
    //         resolve();
    //       },
    //     );
    // });
  }
}
