import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/*
 Codice fiscale del dipendente, come sul server (GestionePresenzeCodiceFiscale): 16 caratteri con il carattere
 di controllo (anche con omocodia) oppure 11 cifre (codice provvisorio, controllo della partita IVA).
*/

const FORMATO_16 = /^[A-Z]{6}[0-9LMNPQRSTUV]{2}[ABCDEHLMPRST][0-9LMNPQRSTUV]{2}[A-Z][0-9LMNPQRSTUV]{3}[A-Z]$/;
const FORMATO_11 = /^[0-9]{11}$/;
const DISPARI_CIFRE = [1, 0, 5, 7, 9, 13, 15, 17, 19, 21];
const DISPARI_LETTERE = [1, 0, 5, 7, 9, 13, 15, 17, 19, 21, 2, 4, 18, 20, 11, 3, 6, 8, 12, 14, 16, 10, 22, 25, 24, 23];

/** maiuscolo e senza spazi */
export function normalizzaCodiceFiscale(valore: string | null | undefined): string {
  return (valore ?? '').replace(/\s/g, '').toUpperCase();
}

/** codice gia' normalizzato: formato e carattere di controllo */
export function isCodiceFiscaleValido(cf: string): boolean {
  if (FORMATO_16.test(cf)) {
    let somma = 0;
    for (let i = 0; i < 15; i++) {
      const c = cf[i];
      const cifra = c >= '0' && c <= '9';
      if (i % 2 === 0)
        somma += cifra ? DISPARI_CIFRE[+c] : DISPARI_LETTERE[c.charCodeAt(0) - 65];
      else
        somma += cifra ? +c : c.charCodeAt(0) - 65;
    }
    return String.fromCharCode(65 + somma % 26) === cf[15];
  }
  if (FORMATO_11.test(cf)) {
    let somma = 0;
    for (let i = 0; i < 10; i++) {
      let n = +cf[i];
      if (i % 2 === 1) {
        n *= 2;
        if (n > 9) n -= 9;
      }
      somma += n;
    }
    return (10 - somma % 10) % 10 === +cf[10];
  }
  return false;
}

/** validatore del form: vuoto = valido (il codice fiscale e' facoltativo); errore "codiceFiscale" */
export function codiceFiscaleValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const cf = normalizzaCodiceFiscale(control.value);
    return cf.length === 0 || isCodiceFiscaleValido(cf) ? null : { codiceFiscale: true };
  };
}
