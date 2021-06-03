import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-deviceform',
  templateUrl: './deviceform.component.html',
  styleUrls: ['./deviceform.component.less']
})
export class DeviceformComponent implements OnInit {

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
