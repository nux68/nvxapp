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

  fromJSONString<T>(jsonString: string, objClass: new () => T): T {
    // Parse la stringa JSON in un oggetto
    const jsonObject = JSON.parse(jsonString);

    // Crea un'istanza della classe fornita
    const objInstance = new objClass();

    // Copia le proprietà dal JSON all'oggetto istanza
    Object.assign(objInstance, jsonObject);

    return objInstance;
  }
  //const timbratura = GenericHelper.fromJSONString(jsonString, Dip_GG_TimbraturaModel);

  DateTimeCurr_To_ISOString(): string {
    const now = new Date();
    const localOffset = now.getTimezoneOffset() * 60000; // Offset in millisecondi
    const localTime = new Date(now.getTime() - localOffset).toISOString().slice(0, 16);

    return localTime;
  }

  DateTime_To_ddmmyyyy_hhmm(date: Date): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;

  }

  DateTime_To_ddmmyyyy(date: Date): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    //const hours = date.getHours().toString().padStart(2, '0');
    //const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${day}/${month}/${year}`;

  }

  DateTime_To_hhmm(date: Date): string {
    //const day = date.getDate().toString().padStart(2, '0');
    //const month = (date.getMonth() + 1).toString().padStart(2, '0');
    //const year = date.getFullYear();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;

  }
}
