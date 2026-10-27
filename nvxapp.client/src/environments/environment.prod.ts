export const environment = {
  production: true,

  signalR: {
    useSignalR: true
  },


  // URL relative: in produzione il client e' servito dal server stesso (wwwroot), quindi API e hub SignalR
  // hanno la stessa origine della pagina. La stessa build vale per qualsiasi server, porta, dominio o http/https.
  // (se un giorno si crea l'app nativa con Capacitor servira' di nuovo un indirizzo assoluto)
  remoteData: {
    apiUri: '/api/',
    signalrUri: '/'
  },
};
