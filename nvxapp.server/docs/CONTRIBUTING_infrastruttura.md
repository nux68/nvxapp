# Contribution Guidelines

This document provides guidelines for contributing to the nvxapp project. Following these guidelines helps maintain the quality and consistency of the codebase.

## Architectural Guidelines

### Backend Development

- **Data Mapping**: For mapping between data entities and Data Transfer Objects (DTOs) in the backend, always use AutoMapper profiles. Do not implement manual mapping logic directly within service classes. This ensures that mapping logic is centralized, reusable, and consistent.

### Frontend Development

- **Component Structure**: Edit page components must inherit from `BasePageConfirmCancelComponent` to ensure a consistent user experience and behavior for confirmation and cancellation actions.
- **UI Consistency**: List pages should use the `app-generic-list` component to maintain a uniform look and feel across the application



