import { Component, OnInit } from '@angular/core';
import { IMqttMessage, MqttModule, IMqttServiceOptions, MqttService } from 'ngx-mqtt';
import { Subscription } from 'rxjs';
export const MQTT_SERVICE_OPTIONS: IMqttServiceOptions = {
  hostname: 'broker.emqx.io', // mqtt 服务器ip
  port: 8083, // mqtt 服务器端口
  path: '/mqtt'
};
@Component({
  selector: 'app-mqtttester',
  templateUrl: './mqtttester.component.html',
  styleUrls: ['./mqtttester.component.less']
})
export class MqtttesterComponent implements OnInit {
  private subscription: Subscription;
  constructor(private _mqttService: MqttService) {
    this.subscription = this._mqttService.observe('testtopic/test').subscribe((message: IMqttMessage) => {
      console.log(message.payload.toString());
    });
    this.subscription = this._mqttService.observe('testtopic/agteletronic/temperatura').subscribe((message: IMqttMessage) => {
      console.log(message.payload.toString());
    });
    this.subscription = this._mqttService.observe('testtopic/miniprogram').subscribe((message: IMqttMessage) => {
      console.log(message.payload.toString());
    });
    this.subscription = this._mqttService.observe('testtopic/react').subscribe((message: IMqttMessage) => {
      console.log(message.payload.toString());
    });
  }

  ngOnInit(): void {
    console.log(this._mqttService);

  //   this._mqttService.connect(MQTT_SERVICE_OPTIONS);

  //   this._mqttService.unsafePublish('devices/61714059796676/rpc/request/sos/dc68fa0371-017', 'aaaaa');
  }
}
