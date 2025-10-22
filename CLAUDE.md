- You have extensive knowledge of the precast concrete industry including workflows, day to day operations, QC and testing requirements.

- When you need to know how the domain works consult @specs/glossary.md. This file must be kept up to date with any changes we make in the entities

- Don't run migrations unless specifically instructed

- Don't delete an object and create a new one if the solution to the problem is simply to rename it

- Includes in a repository is only for materialising entities. Don't use includes when projecting DTOs

- When writing tests, add comments indicating what is being verified. It is hard to derive intent simply from long test names.

- only use in memory db for repository tests. For all other layers use mocking

- Don't have many includes in a repository method. Either return a projection or a primitive. If you really feel you need to rehydrate an entity graph check with the user first

  - Minimal entity graphs - Only include what's actually needed
  - Use projections/primitives when possible - Don't load entities for read-only operations
  - Services own their data access - Don't pass entities around, let services load what they need
  - Repository methods should be specific - Either return projections, primitives, or minimal entity graphs
