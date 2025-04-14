import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class StringHelperService {

  constructor() { }

  removeSpecialCharacters(str: string): string {
      return str.replace(/[^a-zA-Z0-9]/g, '');
  }

}
