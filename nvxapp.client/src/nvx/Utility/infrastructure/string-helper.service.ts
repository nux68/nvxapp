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


}
