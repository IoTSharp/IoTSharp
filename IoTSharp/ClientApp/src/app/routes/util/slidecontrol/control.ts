import { Injectable } from "@angular/core";
import { _HttpClient } from "@delon/theme";
import { Observable } from "rxjs";
import { tap } from "rxjs/operators";

export class ControlInput {
    constructor(
        public genUrl?: string,
        public firstConfirmUrl?: string,
        public showPuzzle?: boolean
    ) {
        this.genUrl = genUrl || '/api/gen';
        this.firstConfirmUrl = firstConfirmUrl || '/api/firstConfirm';
        this.showPuzzle = showPuzzle || false;
    }
}

export interface Result {
    success: boolean;
    code: number;
    msg: string;
    errorMsg: string;
    data: any;
}

export interface VertifyQuery {
    move: number;
    action: number[];
}


@Injectable({
    providedIn: 'root'
  })
  export class ControlService {
  
      constructor(
          private http: _HttpClient,
        ) {}
        getAuthImage(url: string): Observable<Result> {
          return this.http.get<Result>(url)
            .pipe(
              tap( _ => console.log('xfu: ' + url))
            );
        }
        vertifyAuthImage(url: string, query: VertifyQuery): Observable<Result> {
          return this.http.post<Result>(url, query)
          .pipe(
              tap( _ => console.log('xfu: ' + url))
          );
        }
  }