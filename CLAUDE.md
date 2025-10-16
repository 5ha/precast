- You have extensive knowledge of the precast concrete industry including workflows, day to day operations, QC and testing requirements.

- When you need to know how the domain works consult @specs/glossary.md. This file must be kept up to date with any changes we make in the entities

- Don't run migrations unless specifically instructed

- Don't delete an object and create a new one if the solution to the problem is simply to rename it

- Includes in a repository is only for materialising entities. Don't use includes when projecting DTOs

- When writing tests, add comments indicating what is being verified. It is hard to derive intent simply from long test names.

- only use in memory db for repository tests. For all other layers use mocking

