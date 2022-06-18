import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { appmessage } from 'src/app/models/appmessage';

@Component({
  selector: 'app-forkdialog',
  templateUrl: './forkdialog.component.html',
  styleUrls: ['./forkdialog.component.less']
})
export class ForkdialogComponent implements OnInit {

  loading = false;
  @Input() record: NzSafeAny;
  form!: FormGroup;

  ok(): void {
    this.http.post<appmessage<any>>('api/rules/fork', this.form.value).subscribe(
      {
        next: (next) => {

          this.modal.destroy(next);
        },
        error: (error) => { },
        complete: () => { },
      }
    );
  }

  cancel(): void {
    this.modal.destroy();
  }
  constructor(private fb: FormBuilder, private modal: NzModalRef, private http: _HttpClient) { }

  ngOnInit(): void {
    console.log(this.record);
    this.form = this.fb.group({
      ruleId: [this.record.ruleId, []],
      name: ['copy of ' + this.record.name, [Validators.required]],
      ruledesc: [this.record.ruledesc, []],
    });
  }
  submit() {

  }
}
