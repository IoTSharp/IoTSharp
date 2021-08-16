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
    private httpClient: HttpClient,
    @Inject(DA_SERVICE_TOKEN) private tokenService: ITokenService,
  ) {
    iconSrv.addIcon(...ICONS_AUTO, ...ICONS);
  }

  load(): Promise<void> {
    // only works with promises
    // https://github.com/angular/angular/issues/15088

    //如果不是mock数据千万不要中途调用 resolve() subscribe()，直接返回Promise()就可以了，有些奇奇怪怪的问题（已知在刷新页面后，再点击其他窗口，ACL数据会丢失，所有受ACL控制的按钮会消失）,调用远端也不要zip合并，任意一个错误将导致所有请求执行CatchError，逻辑写起来就麻烦些

    var token = this.tokenService;

    if (token && token.get() && token.get()?.token) {
      return concat(
        this.httpClient.get(`assets/tmp/i18n/${this.i18n.defaultLang}.json`).pipe(
          map((langData) => {
            this.translate.setTranslation(this.i18n.defaultLang, langData);
            this.translate.setDefaultLang(this.i18n.defaultLang);
          }),
        ),
        this.httpClient.get('api/Menu/GetProfile').pipe(
          map((appData) => {
            const res = appData as NzSafeAny;

            this.settingService.setApp({ name: 'IotSharp', description: 'IotSharp' });
            this.settingService.setData('drawerconfig', { width: 720, nzMaskClosable: false });
            this.settingService.setUser({
              name: res.result.username,
              avatar: '',
              email: res.result.email,
            });
            var ACL = [];
            if (!res.result.funcs) {
              for (var i = 0; i < 500; i++) {
                ACL = [...ACL, i];
              }
            } else {
              this.aclService.setAbility(res.result.funcs);
            }
            //this.aclService.setAbility(ACL);
            this.aclService.setFull(false); //开启ACL

            var menu = [
              {
                text: '主导航1',
                i18n: 'menu.main',
                group: true,
                hideInBreadcrumb: true,
                children: [
                  {
                    text: '仪表盘',
                    i18n: 'menu.dashboard',
                    icon: 'anticon-dashboard',
                    children: [
                      {
                        text: '仪表盘V1',
                        link: '/dashboard/v1',
                        i18n: 'menu.dashboard.v1',
                      },
                    ],
                  },
                  {
                    text: '租户管理',
                    icon: 'anticon-rocket',
                    shortcutRoot: true,
                    i18n: 'menu.tenant.tenantmanage',
                    children: [
                      {
                        text: '租户列表',
                        link: '/iot/tenant/tenantlist',
                        i18n: 'menu.tenant.tenantlist',
                      },
                    ],
                  },
                  {
                    text: '客户管理',
                    icon: 'anticon-appstore',
                    i18n: 'menu.customer.customermanage',
                    children: [
                      {
                        text: '客户列表',
                        link: '/iot/customer/customerlist',
                        i18n: 'menu.customer.customerlist',
                      },
                    ],
                  },
                  {
                    text: '设备管理',
                    icon: 'anticon-appstore',
                    i18n: 'menu.device.devicemanage',
                    children: [
                      {
                        text: '设备列表',
                        link: '/iot/device/devicelist',
                        i18n: 'menu.device.devicelist',
                      },
                      {
                        text: '设计',
                        link: '/iot/device/devicegraph',
                        //   i18n: 'menu.device.devicelist',
                      },
                      {
                        text: '场景',
                        link: '/iot/device/devicescene',
                        //   i18n: 'menu.device.devicelist',
                      },

                      {
                        text: '规则',
                        link: '/iot/flow/flowlist',
                        //   i18n: 'menu.device.devicelist',
                      },
                    ],
                  },

                  {
                    text: '资源',
                    icon: 'anticon-appstore',
                    children: [
                      {
                        text: '字典分组',
                        link: '/iot/dictionary/dictionarygrouplist',
                        //   i18n: 'menu.device.devicelist',
                      },
                      {
                        text: '字典',
                        link: '/iot/dictionary/dictionarylist',
                        //   i18n: 'menu.device.devicelist',
                      },
                      {
                        text: '国际化',
                        link: '/iot/resouce/i18nlist',
                        //   i18n: 'menu.device.devicelist',
                      },
                      {
                        text: '表单',
                        link: '/iot/util/dynamicformlist',
                        //   i18n: 'menu.device.devicelist',
                      },
                    ],

                    //   i18n: 'menu.device.devicelist',
                  },
                ],
              },
            ];
            if (!res.result.menu) {
              res.data.menu = menu;
            }
            this.menuService.add(res.result.menu);
            this.titleService.default = '';
            this.titleService.suffix = res.result.appName;
          }),
        ),
      ).toPromise();
    } else {
      return concat(
        this.httpClient.get(`assets/tmp/i18n/${this.i18n.defaultLang}.json`).pipe(
          map((langData) => {
            this.translate.setTranslation(this.i18n.defaultLang, langData);
            this.translate.setDefaultLang(this.i18n.defaultLang);
          }),
        ),
      ).toPromise();
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
