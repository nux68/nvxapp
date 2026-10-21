import { provideZoneChangeDetection } from "@angular/core";
import { platformBrowser } from '@angular/platform-browser';
import { setAssetPath as setIonicAssetPath } from '@ionic/core/components';
import { setAssetPath } from 'ionicons/components';

import { AppModule } from './app/app.module';

// Con @ionic/angular (standalone) le icone non registrate con addIcons() vengono cercate in svg/<nome>.svg
// a partire da questo percorso: la cartella svg/ viene copiata da angular.json. Serve perche' molti nomi
// di icona arrivano dai dati (menu, bottoni) e non si possono registrare uno per uno.
// Va impostato su entrambi: ion-icon puo' essere registrato da ionicons o dalla copia interna di @ionic/core.
setAssetPath(document.baseURI);
setIonicAssetPath(document.baseURI);

platformBrowser().bootstrapModule(AppModule, { applicationProviders: [provideZoneChangeDetection()], })
  .catch(err => console.log(err));
