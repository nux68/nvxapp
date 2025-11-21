# Contribution Guidelines

This document provides guidelines for contributing to the nvxapp project. Following these guidelines helps maintain the quality and consistency of the codebase.

## Architectural Guidelines

- **Solution Structure**: Il progetto è organizzato in più soluzioni per separare le responsabilità. Le soluzioni principali sono:
  - `nvxapp.server`: Contiene il backend dell'applicazione.
  - `nvxapp.data`: Contiene l'accesso ai dati e le entità.sd sd fsd 
  - `nvxapp.service`: Contiene la logica di business e i servizi.
  - `nvxapp.client`: Contiene il frontend dell'applicazione. Ed è scritto in Angular / Ionic

- **Branching**: Il progetto si avvle di 2 branc "logici" 
- `Infrastructure`: Contiene tutte le funzioni comuni che saranno condivise con gli altri branch.
- `Infrastructure_Dev`: Contiene il codice in fase di sviluppo di Infrastructure.
- `AttendanceTracking`: Contiene il dell'applicativo specifico per il tracciamento delle presenze.
- `AttendanceTracking_Dev`: Contiene il codice in fase di sviluppo di AttendanceTracking.
- **marging**: Il merging avviene sempre da Infrastructure verso AttendanceTracking e non viceversa
	


### Backend Development

- **Data Mapping**: For mapping between data entities and Data Transfer Objects (DTOs) in the backend, always use AutoMapper profiles. Do not implement manual mapping logic directly within service classes. This ensures that mapping logic is centralized, reusable, and consistent.

### Frontend Development

- **Component Structure**: Edit page components must inherit from `BasePageConfirmCancelComponent` to ensure a consistent user experience and behavior for confirmation and cancellation actions.
- **UI Consistency**: List pages should use the `app-generic-list` component to maintain a uniform look and feel across the application


- [Infrastruttura Guidelines](./CONTRIBUTING_infrastruttura.md)
- [Solution client Guidelines](./CONTRIBUTING_solution_client.md)
- [Solution server Guidelines](./CONTRIBUTING_nvxapp_server.md)
- [Solution data Guidelines](./CONTRIBUTING_nvxapp_data.md)
- [Solution service Guidelines](./CONTRIBUTING_nvxapp_service.md)

