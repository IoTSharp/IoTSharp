import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzModalRef } from 'ng-zorro-antd/modal';

@Component({
  selector: 'app-createdeviceform',
  templateUrl: './createdeviceform.component.html',
  styleUrls: ['./createdeviceform.component.less']
})
export class CreatedeviceformComponent implements OnInit {
  constructor(private nzModalRef:NzModalRef ,  private fb: FormBuilder,private _httpClient: _HttpClient,) { }
  id:string
  form!: FormGroup;
  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required]],
  //    produceId: [this.id, []],
    
    });
  }

  submit(){

   this._httpClient.post('api/Devices/produce/'+this.id,this.form.value).subscribe({
   next:next=>{

if(next.code==10000){
  this.nzModalRef.close(next);
}else{
  this.nzModalRef.close(next);
}

   },
   error:error=>{},
   complete:()=>{},
   })
  }

  cancel(){
    this.nzModalRef.close();
  }
}
