/*
 Salvataggio di un file scaricato dal server (blob): link temporaneo con l'attributo download e clic simulato.
 L'indirizzo del blob si libera solo dopo un po': liberandolo subito dopo il clic i browser degli smartphone
 (soprattutto con l'app installata) a volte non fanno in tempo a leggerlo e il download non parte, senza errori.
*/
export function scaricaBlob(blob: Blob, nomeFile: string): void {
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = nomeFile;
  a.rel = 'noopener';
  a.style.display = 'none';
  document.body.appendChild(a);
  a.click();
  setTimeout(() => {
    a.remove();
    window.URL.revokeObjectURL(url);
  }, 60_000);
}

/*
 Download di un indirizzo del server che risponde con un allegato (es. collegamento temporaneo al file di un job):
 una normale navigazione, senza file in memoria. E' il modo piu' affidabile nell'app installata sul telefono.
*/
export function apriDownload(url: string, nomeFile: string): void {
  const a = document.createElement('a');
  a.href = url;
  a.download = nomeFile;
  a.rel = 'noopener';
  a.style.display = 'none';
  document.body.appendChild(a);
  a.click();
  setTimeout(() => a.remove(), 1000);
}
