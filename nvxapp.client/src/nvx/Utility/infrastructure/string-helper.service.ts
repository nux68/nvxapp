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

  DateCurr_To_ISOString(): string {
    const now = new Date();
    const localOffset = now.getTimezoneOffset() * 60000; // Offset in millisecondi
    const localTime = new Date(now.getTime() - localOffset).toISOString().slice(0, 16);

    return localTime;
  }

  DateCustom_To_ISOString(year: number, month: number, day: number, hh: number, mm: number): string {

    const customDate = new Date(year, month - 1, day, hh, mm); // Nota: i mesi sono 0-based, quindi sottrai 1

    // Calcola l'offset locale
    const localOffset = customDate.getTimezoneOffset() * 60000; // Offset in millisecondi

    // Adatta l'orario al fuso orario locale
    const localTime = new Date(customDate.getTime() - localOffset).toISOString().slice(0, 16);

    return localTime; // Ritorna il risultato formattato
  }

  Date_To_S_ddmmyyyy_hhmm(date: Date): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;

  }

  Date_To_S_ddmmyyyy(date: Date): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    //const hours = date.getHours().toString().padStart(2, '0');
    //const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${day}/${month}/${year}`;

  }

  Date_To_S_hhmm(date: Date): string {
    //const day = date.getDate().toString().padStart(2, '0');
    //const month = (date.getMonth() + 1).toString().padStart(2, '0');
    //const year = date.getFullYear();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;

  }

  Date_S_ddmmyyyy_hhmm_To_Date(dateString: string): Date {
    const [datePart, timePart] = dateString.split(" ");

    // Estrai giorno, mese e anno dalla parte data
    const [day, month, year] = datePart.split("/").map(Number);

    // Estrai ore e minuti dalla parte orario
    const [hours, minutes] = timePart.split(":").map(Number);

    // Crea un oggetto Date
    const date = new Date(year, month - 1, day, hours, minutes);
    return date;

  }

  Date_S_ddmmyyyy_hhmm_To_ISOString(dateString: string): string {

    const date = this.Date_S_ddmmyyyy_hhmm_To_Date(dateString);

    const isoString = date.toISOString();

    return isoString;

  }

 

}
