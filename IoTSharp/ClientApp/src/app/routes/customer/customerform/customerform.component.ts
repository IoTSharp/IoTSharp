
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
import { Guid } from 'guid-typescript';
@Component({
  selector: 'app-customerform',
  templateUrl: './customerform.component.html',
  styleUrls: ['./customerform.component.less']
})
export class CustomerformComponent implements OnInit {

  isManufactorLoading: Boolean = false;

  optionList: any;
  @Input() params: any = {
    id: '-1',
    tenantId: '-1'
  };




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

    console.log(this.params)
    this.form = this.fb.group({
      name: [null, [Validators.required]],
      id: [Guid.create().toString(), []],
      email: [null, []],
      phone: [null, []],
      country: [null, []],
      province: [null, []],
      city: [null, []],
      street: [null, []],
      address: [null, []],
      zipCode: [null, []],
      tenantID: [this.params.tenantId, []],
    });


    if (this.params.id !== '-1') {
      this._httpClient.get('api/Customers/' + this.params.id).subscribe(
        (x) => {
          this.form.patchValue(x);
        },
        (y) => { },
        () => { },
      );
    }
  }

  submit() {
    this.submitting = true;

    if (this.params.id !== "-1") {
      this._httpClient.put("api/Customers/" + this.form.value.id, this.form.value).subscribe(x => { this.submitting = false; }, y => { }, () => { this.submitting = false; });
    } else {
      this._httpClient.post("api/Customers", this.form.value).subscribe(x => { this.submitting = false; }, y => { }, () => { this.submitting = false; });
    }


  }
  close(): void {
    this.drawerRef.close(this.params);
  }

}
