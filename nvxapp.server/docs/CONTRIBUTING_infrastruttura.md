# Guidelines

This document provides guidelines for contributing to the nvxapp project. Following these guidelines helps maintain the quality and consistency of the codebase.

## Architectural Guidelines

### Backend Development

- **Data Mapping**: For mapping between data entities and Data Transfer Objects (DTOs) in the backend, always use AutoMapper profiles. Do not implement manual mapping logic directly within service classes. This ensures that mapping logic is centralized, reusable, and consistent.

### Frontend Development

- **Component Structure**: Edit page components must inherit from `BasePageConfirmCancelComponent` to ensure a consistent user experience and behavior for confirmation and cancellation actions.
- **UI Consistency**: List pages should use the `app-generic-list` component to maintain a uniform look and feel across the application




### Line guida generazione files

Sviluppo Backend
1.	Creare l'Entità:
	1. Definisci la classe dell'entità C# che rappresenta la nuova tabella.
	2. Posiziona il file nel progetto nvxapp.server.data, ad esempio in Entities/Tenant/GestionePresenze/
	3. nvxapp.server.data\Entities\Tenant\GestionePresenze\

2.	Aggiornare il DbContext:
	1. Aggiungi una proprietà DbSet<T> per la nuova entità nel file ApplicationDbContext_GestionePresenze.cs.
	2. Il file da modificare è nvxapp.server.data\Infrastructure\ApplicationDbContext_GestionePresenze.cs.

3.	Creare e Applicare la Migrazione:
	1. Genera una nuova migrazione di Entity Framework per applicare le modifiche al database.
	2. Esegui il comando: Add-Migration <NomeMigrazione>
	3. Applica la migrazione al database.
	4. Esegui il comando: Update-Database
	5. Le migrazioni vengono generate nella cartella nvxapp.server.data\Migrations\.

4.	Creare i Modelli (DTO):
	1. Definisci le classi DTO (Data Transfer Object) che verranno utilizzate per trasferire i dati tra il server e il client.
	2. Posiziona i file nel progetto nvxapp.server.service, ad esempio in Models/Tenant/GestionePresenze/NOMETABELLA/Models.
	3. nvxapp.server.service\Models\Tenant\GestionePresenze\NOMETABELLA\Models\

5.	Configurare AutoMapper:
	1. Come indicato in CONTRIBUTING_infrastruttura.md, crea un profilo di mapping per mappare l'entità al suo DTO corrispondente.
	2. Posiziona il file nel progetto nvxapp.server.service nella cartella Mappers.
	3. nvxapp.server.service\Mappers\Tenant\GestionePresenze\

6.	Creare il Servizio:
	1. Implementa la logica di business (lettura, scrittura, aggiornamento) in una classe di servizio.
	2. Posiziona il file nel progetto nvxapp.server.service nella cartella Services. Il servizio verrà registrato automaticamente per la dependency injection.
	3. nvxapp.server.service\Services\Tenant\GestionePresenze\NOMETABELLA

7.	Creare il Controller:
	1. Crea un controller API per esporre i metodi del servizio come endpoint HTTP.
	2. Posiziona il file nel progetto nvxapp.server nella cartella Controllers.
	3. nvxapp.server\Controllers\Tenant\GestionePresenze\

Sviluppo Frontend (Client)

1.	Creare i Modelli TypeScript:
	1. Seguendo l'esempio in CONTRIBUTING_nvxapp_client.md, crea un file ...-model.ts con le interfacce per l'input e l'output del servizio.
	2. Posiziona il file in nvxapp.client/src/nvx/ClientServer-Service/GestionePresenze/NOMETABELLA/Models/.
	3. nvxapp.client\src\nvx\ClientServer-Service\GestionePresenze\NOMETABELLA\Models\

2.	Creare il Servizio Angular:
	1. Crea un servizio Angular per richiamare gli endpoint dell'API backend.
	2. Posiziona il file in nvxapp.client/src/nvx/ClientServer-Service/GestionePresenze/NOMETABELLA/.
	3. nvxapp.client\src\nvx\ClientServer-Service\GestionePresenze\NOMETABELLA\

3.	Creare la Pagina di Elenco:
	1. Crea un nuovo componente Angular per la visualizzazione dell'elenco.
	2. Nel template HTML, utilizza il componente <app-generic-list> come specificato in CONTRIBUTING_infrastruttura.md e <app-page-toolbar> come da CONTRIBUTING_nvxapp_client.md.
	3. vxapp.client\src\nvx\pages\GestionePresenze\NOMETABELLA\

4.	Creare la Pagina di Modifica/Inserimento:
	1. Crea un nuovo componente Angular per la modifica e l'inserimento dei dati.
	2. Nel template HTML, utilizza i componenti <app-page-toolbar> e <app-page-buttonbar>.
	3. La classe del componente deve ereditare da BasePageConfirmCancelComponent, come richiesto da CONTRIBUTING_infrastruttura.md.
	4. vxapp.client\src\nvx\pages\GestionePresenze\NOMETABELLA\

5.	Aggiornare il Routing:
	1. Aggiungi le rotte per le nuove pagine di elenco e modifica nel file RouteAttendanceTrackingService.ts (o RouteInfrastructureService.ts a seconda del contesto).
	3. Il file da modificare è nvxapp.client\src\nvx\utility\GestionePresenze\route-attendance-tracking.service.ts.

6.	Aggiornare il Menu di Navigazione:
	1. Aggiungi un nuovo elemento di menu per accedere alla pagina di elenco nel file MainMenuAttendanceTrackingService.ts (o MainMenuInfrastructureService.ts).
	2. Il file da modificare è nvxapp.client\src\nvx\utility\GestionePresenze\main-menu-attendance-tracking.service.ts.



	- [Home](./CONTRIBUTING.md)