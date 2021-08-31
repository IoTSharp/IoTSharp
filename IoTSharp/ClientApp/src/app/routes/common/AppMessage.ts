export interface AppMessage {
  errType?: number;
  errMessage?: string;
  result?: any;
  errLevel?: string;
  isVisble?: boolean;
}
export class appmessage<T> {
  errType?: number;
  errMessage?: string;
  result?: T;
  errLevel?: string;
  isVisble?: boolean;
}
