import { Component, Inject, OnInit } from '@angular/core';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';

@Component({
  selector: 'layout-passport',
  templateUrl: './passport.component.html',
  styleUrls: ['./passport.component.less'],
})
export class LayoutPassportComponent implements OnInit {
  links = [
    {
      title: '文档',
      href: 'https://docs.iotsharp.net/',
    },
    {
      title: '源码',
      href: 'https://github.com/IoTSharp/IoTSharp',
    },
    {
      title: '容器',
      href: 'https://hub.docker.com/r/iotsharp/iotsharp',
    },
  ];

  constructor(@Inject(DA_SERVICE_TOKEN) private tokenService: ITokenService) {}

  ngOnInit(): void {
    this.tokenService.clear();
  }
}
