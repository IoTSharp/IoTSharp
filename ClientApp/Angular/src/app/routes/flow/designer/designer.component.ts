import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ModalHelper, _HttpClient } from '@delon/theme';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder } from '@angular/forms';
import { Observable, Observer, of } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { DiagramComponent } from '../diagram/diagram.component';
@Component({
  selector: 'app-designer',
  templateUrl: './designer.component.html',
  styleUrls: ['./designer.component.less'],
})
export class DesignerComponent implements OnInit {
  @ViewChild('diagram', { static: true })
  private diagram!: DiagramComponent;
  DefinitionsId: number = 0;
  constructor(
    private _router: ActivatedRoute,
    private router: Router,
    private _formBuilder: FormBuilder,
    private _httpClient: _HttpClient,
  ) {}

  ngOnInit(): void {
    // this._router.queryParams
    //   .pipe(
    //     mergeMap((x) => {
    //       this.DefinitionsId = x.Id;
    //       if (x.Id !== '-1') {
    //         this.title = '开始设计';
    //         this.diagramUrl = 'api/manage/workflow/get?id=' + x.Id;
    //         return of([]);
    //       } else {
    //         return of([]);
    //       }
    //       this.title = '开始设计';
    //     }),
    //     catchError(() => {
    //       return of([]);
    //     }),
    //   )
    //   .subscribe();
  }

  title = '开始设计';
  diagramUrl = '';
  importError?: Error;
  handleImported(event: any) {
    const { type, error, warnings } = event;

    if (type === 'success') {
      console.log(`Rendered diagram (%s warnings)`, warnings);
    }

    if (type === 'error') {
      console.error('Failed to render diagram', error);
    }

    this.importError = error;
  }

  newdiagram() {
    this.diagramUrl = '';
    this.diagram.loadUrl('');
  }

  savediagram() {
    this.diagram.savediagram();
  }
}
