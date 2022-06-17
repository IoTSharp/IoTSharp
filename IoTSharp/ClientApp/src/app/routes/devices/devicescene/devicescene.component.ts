import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { DeviceSceneService } from 'src/app/services/devicescene.service';

@Component({
  selector: 'app-devicescene',
  templateUrl: './devicescene.component.html',
  styleUrls: ['./devicescene.component.less']
})
export class DevicesceneComponent implements OnInit {

  @ViewChild('rendererCanvas', { static: true })
  public rendererCanvas!: ElementRef<HTMLCanvasElement>;

  public constructor(private dsServ: DeviceSceneService) {}

  public ngOnInit(): void {
    this.dsServ.createScene(this.rendererCanvas);
    //  this.engServ.animate();
  }
}
