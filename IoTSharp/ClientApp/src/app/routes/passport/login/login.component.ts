import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnDestroy, OnInit, Optional, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { I18NService, StartupService } from '@core';
import { ReuseTabService } from '@delon/abc/reuse-tab';
import { DA_SERVICE_TOKEN, ITokenService, SocialOpenType, SocialService } from '@delon/auth';
import { ALAIN_I18N_TOKEN, SettingsService, _HttpClient } from '@delon/theme';
import { environment } from '@env/environment';

import { NzMessageService } from 'ng-zorro-antd/message';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { NzTabChangeEvent } from 'ng-zorro-antd/tabs';
import { finalize } from 'rxjs/operators';

import { Guid } from 'guid-typescript';
import { SlidecontrolComponent } from '../../util/slidecontrol/slidecontrol.component';
import { ControlInput, VertifyQuery } from '../../util/slidecontrol/control';
@Component({
  selector: 'passport-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.less'],
  providers: [SocialService],
  
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserLoginComponent implements OnDestroy,OnInit {
   isVisible:boolean=false;
   isVertify:boolean=false;
   clientid= Guid.create();
  private query: VertifyQuery;
  public controlInput: ControlInput;
  @ViewChild(SlidecontrolComponent, {static: true})
  slide: SlidecontrolComponent;
  constructor(
    fb: FormBuilder,
    private cdr: ChangeDetectorRef, 
    private router: Router,
    private settingsService: SettingsService,
    private socialService: SocialService,
    @Optional()
    @Inject(ReuseTabService)
    private reuseTabService: ReuseTabService,
    @Inject(DA_SERVICE_TOKEN) private tokenService: ITokenService,
    private startupSrv: StartupService,
    public http: _HttpClient,
    public msg: NzMessageService,
    public notification: NzNotificationService,
   
  ) {
    this.form = fb.group({
      userName: ['iotmaster@iotsharp.net', [Validators.required]],
      password: ['', [Validators.required]],
      // mobile: [null, [Validators.required, Validators.pattern(/^1\d{10}$/)]],
      // captcha: [null, [Validators.required]],
      remember: [true]
    });
  }
  ngOnInit(): void {
    this.controlInput = new ControlInput(
      'api/Captcha/Index?clientid='+this.clientid,
      'api/Captcha/Vertify?clientid='+this.clientid,
      false,
    );
  }
  ngAfterViewInit(): void {
    localStorage.clear();

    this.http.get('api/installer/instance?_allow_anonymous=true').subscribe(
      x => {
        if (x.code === 10000) {
          if (x.data.installed) {
          } else {
            this.router.navigateByUrl('/passport/register?type=install');
          }
        } else {
          this.notification.error('请求错误', 'Api请求不正确');
        }
      },
      error => {
        this.notification.error('请求错误', '系统异常');
      },
      () => {}
    );
  }

  // #region fields

  get userName(): AbstractControl {
    return this.form.controls.userName;
  }
  get password(): AbstractControl {
    return this.form.controls.password;
  }
  get mobile(): AbstractControl {
    return this.form.controls.mobile;
  }
  get captcha(): AbstractControl {
    return this.form.controls.captcha;
  }
  form: FormGroup;

  type = 0;
  error: boolean = false;
  // #region get captcha

  count = 0;
  interval$: any;

  // #endregion

  switch({ index }: NzTabChangeEvent): void {
    this.type = index!;
  }
  successMatch($event){
    
    this.isVertify=true;
    this.isVisible=!this.isVertify

  
       this.signIn($event.move);

  }
  getCaptcha(): void {
    if (this.mobile.invalid) {
      this.mobile.markAsDirty({ onlySelf: true });
      this.mobile.updateValueAndValidity({ onlySelf: true });
      return;
    }
    this.count = 59;
    this.interval$ = setInterval(() => {
      this.count -= 1;
      if (this.count <= 0) {
        clearInterval(this.interval$);
      }
    }, 1000);
  }

  // #endregion


  signIn(move:number){
    this.error = false;
    if (this.type === 0) {
      this.userName.markAsDirty();
      this.userName.updateValueAndValidity();
      this.password.markAsDirty();
      this.password.updateValueAndValidity();
      if (this.userName.invalid || this.password.invalid) {
        return;
      }
    } else {
      this.mobile.markAsDirty();
      this.mobile.updateValueAndValidity();
      this.captcha.markAsDirty();
      this.captcha.updateValueAndValidity();
      if (this.mobile.invalid || this.captcha.invalid) {
        return;
      }
    }

    // 默认配置中对所有HTTP请求都会强制 [校验](https://ng-alain.com/auth/getting-started) 用户 Token
    // 然一般来说登录请求不需要校验，因此可以在请求URL加上：`/login?_allow_anonymous=true` 表示不触发用户 Token 校验
    this.http
      .post('api/Account/Login?_allow_anonymous=true', {
        type: this.type,
        userName: this.userName.value,
        password: this.password.value,
        move:move

      })
      .subscribe(
        x => {
          if (x.code !== 10000) {
            this.error = true;
            this.cdr.detectChanges();


            return;
          }
          // 清空路由复用信息
          this.reuseTabService.clear();
          // 设置用户Token信息
          // TODO: Mock expired value
        var  expired = +new Date() + 1000 * x.data.token.expires_in
          this.tokenService.set({
            token: x.data.token.access_token,
            Authorization: x.data.token.access_token,
            expired: expired,
            name: x.data.userName,
            refreshtoken: x.data.token.refresh_token
          }); 

          this.settingsService.setUser({
            token: x.data.token.access_token,
            name: x.data.userName,
            avatar: './assets/logo.png',
            email: 'iotmaster@iotsharp.net'
          });
          // 重新获取 StartupService 内容，我们始终认为应用信息一般都会受当前用户授权范围而影响
          this.startupSrv.load().subscribe(() => {
            let url = this.tokenService.referrer!.url || '/';
            if (url.includes('/passport')) {
              url = '/';
            }
            this.router.navigateByUrl(url);
          });
        },
        error => {
          this.error = error.message;
        },
        () => {}
      );

  }

  submit(): void {
   this.isVisible=true;
  }

  // #region social

  open(type: string, openType: SocialOpenType = 'href'): void {
    let url = ``;
    let callback = ``;
    // tslint:disable-next-line: prefer-conditional-expression
    if (environment.production) {
      callback = 'https://ng-alain.github.io/ng-alain/#/passport/callback/' + type;
    } else {
      callback = 'http://localhost:4200/#/passport/callback/' + type;
    }
    switch (type) {
      case 'auth0':
        url = `//cipchk.auth0.com/login?client=8gcNydIDzGBYxzqV0Vm1CX_RXH-wsWo5&redirect_uri=${decodeURIComponent(callback)}`;
        break;
      case 'github':
        url = `//github.com/login/oauth/authorize?client_id=9d6baae4b04a23fcafa2&response_type=code&redirect_uri=${decodeURIComponent(
          callback
        )}`;
        break;
      case 'weibo':
        url = `https://api.weibo.com/oauth2/authorize?client_id=1239507802&response_type=code&redirect_uri=${decodeURIComponent(callback)}`;
        break;
    }
    if (openType === 'window') {
      this.socialService
        .login(url, '/', {
          type: 'window'
        })
        .subscribe(res => {
          if (res) {
            this.settingsService.setUser(res);
            this.router.navigateByUrl('/');
          }
        });
    } else {
      this.socialService.login(url, '/', {
        type: 'href'
      });
    }
  }

  // #endregion

  ngOnDestroy(): void {
    if (this.interval$) {
      clearInterval(this.interval$);
    }
  }
}
