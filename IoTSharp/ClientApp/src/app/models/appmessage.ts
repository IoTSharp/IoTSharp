export class appmessage<T> {
  code?: number;
  msg?: string;
  data?: T;
}
export interface pageddata<T> {
  rows: T[];
  total: number;
}
