# Contribution Guidelines

This document provides guidelines for contributing to the nvxapp project. Following these guidelines helps maintain the quality and consistency of the codebase.

## Architectural Guidelines


# Solution Structure
### Il progetto è organizzato in più soluzioni. Le soluzioni principali sono:
- `nvxapp.server`: Contiene il backend dell'applicazione.
- `nvxapp.data`: Contiene l'accesso ai dati e le entità.sd sd fsd 
- `nvxapp.service`: Contiene la logica di business e i servizi.
- `nvxapp.client`: Contiene il frontend dell'applicazione. Ed è scritto in Angular / Ionic

# Branch
### Branching: Il progetto si avvale di 2 branc "logici" 
- `Infrastructure`: Contiene tutte le funzioni comuni che saranno condivise con gli altri branch.
- `Infrastructure_Dev`: Contiene il codice in fase di sviluppo di Infrastructure.
- `AttendanceTracking`: Contiene il dell'applicativo specifico per il tracciamento delle presenze.
- `AttendanceTracking_Dev`: Contiene il codice in fase di sviluppo di AttendanceTracking.
- **marging**: Il merging avviene sempre da Infrastructure verso AttendanceTracking e non viceversa

- **Folder Structure**: Il progetto è organizzato su diverse cartelle ma i nomi di queste assumono un significato particolare i due casi 
- `GestionePresenze`: Quando la cartella contine dati relativi alla gestione presenze, vanno modificate solo nel branch `AttendanceTracking`.
- `Infrastructure`: Quando la cartella contine dati relativi alla gestione presenze, vanno modificate solo nel branch `Infrastructure`.
- `branch corrente`: Quando il brancgh corrente è `Infrastructure` o `Infrastructure_Dev`, la creazione dei nuovi fileavviene solo nelle cartelle `Infrastructure` dell argomento opportuno , a seconda che stia creando files per data,service,page ecc.
- Quando il branch corrente è `AttendanceTracking` o `AttendanceTracking_Dev`, la creazione dei nuovi fileavviene solo nelle cartelle `GestionePresenze` dell argomento opportuno , a seconda che stia creando files per data,service,page ecc.




- [Infrastruttura](./CONTRIBUTING_infrastruttura.md)
- [Solution Client](./CONTRIBUTING_nvxapp_client.md)
- [Solution Server](./CONTRIBUTING_nvxapp_server.md)
- [Solution Data](./CONTRIBUTING_nvxapp_data.md)
- [Solution Service](./CONTRIBUTING_nvxapp_service.md)

