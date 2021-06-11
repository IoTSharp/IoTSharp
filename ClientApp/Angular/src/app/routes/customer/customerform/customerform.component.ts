
import { ModalHelper, _HttpClient } from '@delon/theme';
import { Component, OnInit, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzUploadFile } from 'ng-zorro-antd/upload';
import { Observable, Observer, of } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';
;
import { MyValidators } from '../../common/validators/MyValidators';
import { AppMessage } from '../../common/AppMessage';
@Component({
  selector: 'app-customerform',
  templateUrl: './customerform.component.html',
  styleUrls: ['./customerform.component.less']
})
export class CustomerformComponent implements OnInit {

  isManufactorLoading: Boolean = false;

  optionList: any;
  @Input() id: string = '-1';
  RoleLogo: NzUploadFile[] = [];

  nodes = [];

  title: string = '';

  loading = false;
  avatarUrl?: string;
  constructor(
    private _router: ActivatedRoute,
    private router: Router,
    private _formBuilder: FormBuilder,
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private msg: NzMessageService,
    private drawerRef: NzDrawerRef<string>,
  ) { }
  form!: FormGroup;

  submitting = false;



  ngOnInit() {
    const { nullbigintid } = MyValidators;
    this.form = this.fb.group({
      name: [null, [Validators.required]],
      id: [0, []],
      email: [0, [nullbigintid]],
      phone: [null, []],
      country: [null, []],
      province: [0, []],
      city: [null, []],
      street: [null, []],
      address: [null, []],
      zipCode: [null, []],

    });


    if (this.id !== '-1') {
      this._httpClient.get<AppMessage>('/api/Customers/' + this.id).subscribe(
        (x) => {
          this.form.patchValue(x.Result);
        },
        (y) => { },
        () => { },
      );
    }
  }

  submit() {
    this.submitting = true;

    if (this.id !== "-1") {
      this._httpClient.put<AppMessage>("/api/Customers", this.form.value).subscribe();
    } else {
      this._httpClient.post<AppMessage>("/api/Customers", this.form.value).subscribe();
    }


  }
  close(): void {
    this.drawerRef.close(this.id);
  }

}
