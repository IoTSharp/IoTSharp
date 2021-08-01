import { ChangeDetectorRef } from '@angular/core';
import { Input, ViewChild } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { SFComponent, SFSchema } from '@delon/form';
import { _HttpClient } from '@delon/theme';
import { switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-propform',
  templateUrl: './propform.component.html',
  styleUrls: ['./propform.component.less'],
})
export class PropformComponent implements OnInit {
  @ViewChild('sf', { static: false })
  sf!: SFComponent;
  @Input() params: any = {
    id: '-1',
    customerId: '-1',
  };
  constructor(private http: _HttpClient, private cd: ChangeDetectorRef) {}
  schema: SFSchema = {
    properties: {
      // "field1": {
      //     "type": "string",
      //     "title": "参12222数5",
      //     "maxlength": 20
      // },
    },
  };

  ngOnInit(): void {
    this.http.get<deviceattributeitem[]>('api/Devices/' + this.params.id + '/AttributeLatest').subscribe(
      (next) => {
        var properties: any = {};
        for (var item of next) {
          // switch (item.uielement) {
          //   case 'uielement':
          properties[item.keyName] = {
            type: 'string',
            title: item.keyName,
            // maxLength: item.maxLength,
            //  pattern: item.pattern,
            // ui: {
            //   addOnAfter: item.ui.addOnAfter,
            //   placeholder: item.ui.placeholder,
            // },
            default: item.value,
          };
          //     };
          //     break;
          // }
        }

        this.schema.properties = properties;
        this.sf.refreshSchema();
        this.cd.detectChanges();
      },
      (error) => {},
      () => {},
    );
  }

  submit(value: any) {
    this.http
      .get<deviceidentityinfo>('api/Devices/' + this.params.id + '/Identity')
      .pipe(
        switchMap((deviceidentityinfo: deviceidentityinfo) =>
          this.http.post('api/Devices/' + deviceidentityinfo.identityId + '/Attributes', value),
        ),
      )
      .subscribe(
        (next) => {},
        (error) => {},
        () => {},
      );
  }
}
export interface deviceidentityinfo {
  id: string;
  identityType: any;
  identityId: string;
  identityValue: string;
}

export interface deviceattributeitem {
  keyName: string;
  dataSide: any;
  dateTime: string;
  value: string;
}
