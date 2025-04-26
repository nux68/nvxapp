import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class StringHelperService {

  constructor() { }

  removeSpecialCharacters(str: string): string {
      return str.replace(/[^a-zA-Z0-9]/g, '');
  }

  toJSONString<T>(obj: T): string {
    return JSON.stringify(obj, (key, value) => {
      // Se il valore è una data, convertila in stringa ISO
      return value instanceof Date ? value.toISOString() : value;
    });
  }
}
