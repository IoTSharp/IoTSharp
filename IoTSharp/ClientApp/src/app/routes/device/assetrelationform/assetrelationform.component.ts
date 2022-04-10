import { Component, OnInit, Input } from '@angular/core';
import { appmessage, AppMessage } from '../../common/AppMessage';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
@Component({
  selector: 'app-assetrelationform',
  templateUrl: './assetrelationform.component.html',
  styleUrls: ['./assetrelationform.component.less']
})
export class AssetrelationformComponent implements OnInit {

  title='';
  @Input()
  params: any;
  form!: FormGroup;
  submitting = false;
  constructor(private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private msg: NzMessageService,
    private drawerRef: NzDrawerRef<string>,) { }

  ngOnInit(): void {
    this.form = this.fb.group({
      name: [this.params.name, [Validators.required]],
      description: [this.params.description, []],
      id: [this.params.id, [Validators.required]],
    });


  }

  close() {

    this.drawerRef.close(this.form.value);
  }

  submit() {

    this._httpClient.post('api/asset/editRelation', this.form.value).subscribe(next => {

      if (next?.data) {
        this.close();
      }
    }, error => {}, () => {});

  }
}
