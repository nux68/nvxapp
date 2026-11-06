# Schermate dei manuali utente

I manuali utente in PDF (vedi `nvxapp.server.service/ClientServer-Service/infrastructure/UserManual/UserManual.cs`) mostrano, per ogni voce del menu, la schermata della pagina. Le schermate sono file JPEG in `nvxapp.server.service/Manuals/img/`, incorporati nella DLL: il nome è l'indirizzo della pagina senza la `/` iniziale (le `/` interne diventano `_`), per esempio `/crmpipeline` → `crmpipeline.jpg`.

Quando una pagina cambia aspetto, rigenera le schermate con `capture.js`.

## Come rigenerarle
1. Avvia l'applicazione con dati di esempio (per esempio il server di sviluppo con un database di prova popolato) e annota l'indirizzo (es. `http://localhost:5299`).
2. Avvia Chrome con il debug remoto:
   `chrome --headless=new --remote-debugging-port=9222 --user-data-dir=%TEMP%\chrome_manuali`
3. Lancia lo script con gli utenti che vedono le voci da fotografare (più ruoli = più voci):
   `node capture.js http://localhost:5299 ..\..\..\nvxapp.server.service\Manuals\img SuperUser:1234 CompanyPowerAdmin_1_1_1:1234 User_1_1_1_1:1234`
4. Copia ogni schermata nel ramo che possiede la pagina: le pagine di Infrastructure in `Infrastructure_Dev`, quelle delle presenze in `AttendanceTracking_Dev`, del CRM in `CRM_Dev`, del documentale in `DocumentManagement_Dev` (la cartella `Manuals/img` è la stessa in tutti i rami).

Le schermate si fanno in tema chiaro, solo sull'area della pagina (senza il menu laterale), ridotte all'80%.
