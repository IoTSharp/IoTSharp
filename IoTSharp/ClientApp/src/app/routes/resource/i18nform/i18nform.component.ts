import { HttpClient } from '@angular/common/http';
import { Component, Inject, Input, OnInit } from '@angular/core';
import { AsyncValidatorFn, AbstractControl, FormBuilder, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { I18NService } from '@core';
import { ALAIN_I18N_TOKEN } from '@delon/theme';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { Observable, observable, Observer } from 'rxjs';
import { debounceTime, map, switchMap, take } from 'rxjs/operators';
import { AppMessage } from '../../common/AppMessage';
/*import * as md5 from 'md5';*/ //NPM 安装 md5
@Component({
  selector: 'app-i18nform',
  templateUrl: './i18nform.component.html',
  styleUrls: ['./i18nform.component.less'],
})
export class I18nformComponent implements OnInit {
  form!: FormGroup;
  submitting = false;
  @Input() id: Number = -1;
  constructor(
    @Inject(ALAIN_I18N_TOKEN) private i18n: I18NService,
    private _httpClient: HttpClient,
    private fb: FormBuilder,
    private notification: NzNotificationService,
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      keyName: [null, [Validators.required,], [this.keyNameValidator]],
      valueBG: [null, []],
      valueCS: [null, []],
      valueDA: [null, []],
      valueDEDE: [null, []],
      valueESES: [null, []],
      valueENUS: [null, []],
      valueENGR: [null, []],
      valueELGR: [null, []],
      valueFI: [null, []],
      valueFRFR: [null, []],
      valueHE: [null, []],
      valueHRHR: [null, []],
      valueHU: [null, []],
      valueITIT: [null, []],
      valueJAJP: [null, []],
      valueKOKR: [null, []],
      valueNL: [null, []],
      valuePLPL: [null, []],
      valuePT: [null, []],
      valueSLSL: [null, []],
      valueTRTR: [null, []],
      valueSR: [null, []],
      valueSV: [null, []],
      valueUK: [null, []],
      valueVI: [null, []],
      valueZHCN: [null, []],
      valueZHTW: [null, []],
      resourceType: [1, []],
      resourceKey: [null, []],
      resouceDesc: [null, []],
      id: [0, []],
    });
    if (this.id !== -1) {
      this._httpClient.get<AppMessage>('api/i18n/get?id=' + this.id).subscribe(
        (x) => {
          this.form.patchValue(x.data);
        },
        (y) => {},
        () => {},
      );
    }
  }

  submit() {
    this.submitting = true;
    var uri = this.id > 0 ? 'api/i18n/update' : 'api/i18n/save';
    this._httpClient.post(uri, this.form.value).subscribe(
      (x) => {
        this.submitting = false;
        this.notification.blank(this.i18n.fanyi('message.save.success'), this.i18n.fanyi('message.save.success'), { nzDuration: 0 });
        this.form.reset();
        this.form.patchValue({ Id: 0, ResourceType: 1 });
      },
      (y) => {
        this.submitting = false;
      },
      () => {},
    );
  }

  keyNameValidator = (control: FormControl) => {

    return this._httpClient.get<AppMessage>('api/i18n/checkexist?key=' + control.value)
      .pipe(
        map(x =>
          x.data ? '1111' : null
        )
      )
  }


  


  close() {}

  autotrans(): void {
    //let AppKey = '百度的Key';
    //let AppSecret = '百度的Secret';
    //let salt = new Date().getTime();
    //let query = this.form.value.ValueENUS;
    //var unsign = AppKey + query + salt + AppSecret
    //var sign = MD5(unsign)
    let Url = 'api/i18n/translate?Words=' + this.form.value.valueZHCN;

    this._httpClient.get<AppMessage>(Url).subscribe(
      (x) => {
        for (var i = 0; i < x.data.length; i++) {
          switch (x.data[i].to) {
            case 'en':
              console.log(x.data[i].trans_result[0].dst);
              this.form.patchValue({ valueENUS: x.data[i].trans_result[0].dst });
              this.form.patchValue({ valueENGR: x.data[i].trans_result[0].dst });
              break;
            case 'kor':
              this.form.patchValue({ valueKOKR: x.data[i].trans_result[0].dst });
              break;
            case 'spa':
              this.form.patchValue({ valueESES: x.data[i].trans_result[0].dst });
              break;
            case 'de':
              this.form.patchValue({ valueDEDE: x.data[i].trans_result[0].dst });
              break;
            case 'fra':
              this.form.patchValue({ valueFRFR: x.data[i].trans_result[0].dst });
              break;
            case 'jp':
              this.form.patchValue({ valueJAJP: x.data[i].trans_result[0].dst });
              break;
            case 'it':
              this.form.patchValue({ valueITIT: x.data[i].trans_result[0].dst });
              break;
            case 'tr':
              this.form.patchValue({ valueTRTR: x.data[i].trans_result[0].dst });
              break;
            case 'hrv':
              this.form.patchValue({ valueHRHR: x.data[i].trans_result[0].dst });
              break;
            case 'slo':
              this.form.patchValue({ valueSLSL: x.data[i].trans_result[0].dst });
              break;
            case 'pl':
              this.form.patchValue({ valuePLPL: x.data[i].trans_result[0].dst });
              break;
            case 'cht':
              this.form.patchValue({ valueZHTW: x.data[i].trans_result[0].dst });
              break;
            case 'el':
              this.form.patchValue({ valueELGR: x.data[i].trans_result[0].dst });
              break;
            case 'bul':
              this.form.patchValue({ valueBG: x.data[i].trans_result[0].dst });
              break;
            case 'cs':
              this.form.patchValue({ valueCS: x.data[i].trans_result[0].dst });
              break;
            case 'dan':
              this.form.patchValue({ valueDA: x.data[i].trans_result[0].dst });
              break;
            case 'fin':
              this.form.patchValue({ valueFI: x.data[i].trans_result[0].dst });
              break;
            case 'heb':
              this.form.patchValue({ valueHE: x.data[i].trans_result[0].dst });
              break;
            case 'hu':
              this.form.patchValue({ valueHU: x.data[i].trans_result[0].dst });
              break;
            case 'nl':
              this.form.patchValue({ valueNL: x.data[i].trans_result[0].dst });
              break;
            case 'srp':
              this.form.patchValue({ valueSR: x.data[i].trans_result[0].dst });
              break;
            case 'swe':
              this.form.patchValue({ valueSV: x.data[i].trans_result[0].dst });
              break;
            case 'ukr':
              this.form.patchValue({ valueUK: x.data[i].trans_result[0].dst });
              break;
            case 'vie':
              this.form.patchValue({ valueVI: x.data[i].trans_result[0].dst });
              break;
            case 'pt':
              this.form.patchValue({ valuePT: x.data[i].trans_result[0].dst });
              break;
            default:
          }
        }
      },
      (y) => {},
      () => {},
    );
  }
}
